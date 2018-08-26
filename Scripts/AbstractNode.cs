using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeEditor.Util;
using UnityEngine;

namespace NodeEditor
{
	public class AbstractNode : INode, ISerializationCallbackReceiver, IDisposable
	{
		protected static List<NodeSlot> s_TempSlots = new List<NodeSlot>();
		protected static List<IEdge> s_TempEdges = new List<IEdge>();
		protected static List<PreviewProperty> s_TempPreviewProperties = new List<PreviewProperty>();

		[NonSerialized] private Guid m_Guid;

		[SerializeField] private string m_GuidSerialized;

		[SerializeField] private string m_Name;

		[SerializeField] private DrawState m_DrawState;

		[NonSerialized] private Dictionary<int, ISlot> m_Slots = new Dictionary<int, ISlot>();

		[SerializeField] List<SerializationHelper.JSONSerializedIndexedElement> m_SerializableSlots = new List<SerializationHelper.JSONSerializedIndexedElement>();

		[NonSerialized] private bool m_HasError;

		[NonSerialized] private IGraph m_Owner;

		[SerializeField] private int m_Priority;

		public Identifier tempId { get; set; }

		public IGraph owner => m_Owner;

		OnNodeModified m_OnModified;

		public void RegisterCallback(OnNodeModified callback)
		{
			m_OnModified += callback;
		}

		public void UnregisterCallback(OnNodeModified callback)
		{
			m_OnModified -= callback;
		}

		public void Dirty(ModificationScope scope)
		{
			if (m_OnModified != null)
				m_OnModified(this, scope);
		}

		public Guid guid => m_Guid;

		public string name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		public virtual string documentationURL => null;

		public virtual bool canDeleteNode => true;

		public int priority { get { return m_Priority;}
			set
			{
				if (m_Priority != value)
				{
					m_Priority = value;
					OnPriorityChange();
				}
			}
		}

		private void OnPriorityChange()
		{
			var abstractNodeGraph = owner as AbstractNodeGraph;
			if (abstractNodeGraph != null)
			{
				abstractNodeGraph.SortEdges(guid);
			}
			Dirty(ModificationScope.Node);
		}

		public DrawState drawState
		{
			get { return m_DrawState; }
			set
			{
				m_DrawState = value;
				Dirty(ModificationScope.Node);
			}
		}

		protected IEnumerable<ISlot> slots => m_Slots.Values;

		[SerializeField] bool m_PreviewExpanded = true;

		public bool previewExpanded
		{
			get { return m_PreviewExpanded; }
			set
			{
				if (previewExpanded == value)
					return;
				m_PreviewExpanded = value;
				Dirty(ModificationScope.Node);
			}
		}

		// Nodes that want to have a preview area can override this and return true
		public virtual bool hasPreview => false;

		public virtual PreviewMode previewMode => PreviewMode.Preview2D;

		// Nodes that want to have a preview area can override this and return true

		public virtual bool allowedInSubGraph => true;

		public virtual bool allowedInMainGraph => true;

		public virtual bool allowedInLayerGraph => true;

		public virtual bool hasError
		{
			get { return m_HasError; }
			protected set { m_HasError = value; }
		}

		string m_DefaultVariableName;
		string m_NameForDefaultVariableName;
		Guid m_GuidForDefaultVariableName;

		string defaultVariableName
		{
			get
			{
				if (m_NameForDefaultVariableName != name || m_GuidForDefaultVariableName != guid)
				{
					m_DefaultVariableName = string.Format("{0}_{1}", name, GuidEncoder.Encode(guid));
					m_NameForDefaultVariableName = name;
					m_GuidForDefaultVariableName = guid;
				}
				return m_DefaultVariableName;
			}
		}

		public AbstractNode()
		{
			name = GetType().Name;
			m_DrawState.expanded = true;
			m_Guid = Guid.NewGuid();
			version = 0;
		}

		public Guid RewriteGuid()
		{
			m_Guid = Guid.NewGuid();
			return m_Guid;
		}

		public void GetInputSlots<T>(List<T> foundSlots) where T : ISlot
		{
			foreach (var slot in m_Slots.Values)
			{
				if (slot.isInputSlot && slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public void GetOutputSlots<T>(List<T> foundSlots) where T : ISlot
		{
			foreach (var slot in m_Slots.Values)
			{
				if (slot.isOutputSlot && slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public void GetSlots<T>(List<T> foundSlots) where T : ISlot
		{
			foreach (var slot in m_Slots.Values)
			{
				if (slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public T GetSlotValue<T>(ISlot inputSlot)
		{
			var nodeSlot = inputSlot as NodeSlot;
			if (nodeSlot != null)
			{
				var con = nodeSlot.GetSlotConnectionCache().FirstOrDefault();

				if (con != null)
				{
					var hasValueSlot = con as IHasValue<T>;
					if (hasValueSlot != null)
						return hasValueSlot.value;

					var hasValueBase = con as IHasValue;
					if (hasValueBase != null)
						return (T) hasValueBase.value;
				}

				var hasValueInput = inputSlot as IHasValue<T>;
				if (hasValueInput != null)
					return hasValueInput.value;
			}
			else
			{
				var edge = owner.GetEdge(inputSlot.slotReference);

				if (edge != null)
				{
					var fromSocketRef = edge.outputSlot;
					var fromNode = owner.GetNodeFromGuid<AbstractNode>(fromSocketRef.nodeGuid);
					if (fromNode == null)
						return default(T);

					var slot = fromNode.FindValueOutputSlot<T>(fromSocketRef.slotId);
					if (slot == null)
						return default(T);

					return slot.value;
				}

				var hasValue = inputSlot as IHasValue<T>;
				return hasValue != null ? hasValue.value : default(T);
			}

			return default(T);
		}

		public void GetSlotValues<T>(ISlot inputSlot,IList<T> values)
		{
			var nodeSlot = inputSlot as NodeSlot;
			if (nodeSlot != null)
			{
				var cache = nodeSlot.GetSlotConnectionCache();
				foreach (var slot in cache)
				{
					var hasValueSlot = slot as IHasValue<T>;
					if (hasValueSlot != null)
					{
						values.Add(hasValueSlot.value);
					}
					else
					{
						var hasValueBase = slot as IHasValue;
						if (hasValueBase != null)
							values.Add((T)hasValueBase.value);
					}
				}
			}
			else
			{
				var edges = ListPool<IEdge>.Get();
				owner.GetEdges(inputSlot.slotReference, edges);
				foreach (var edge in edges)
				{
					var fromSocketRef = edge.outputSlot;
					var fromNode = owner.GetNodeFromGuid<AbstractNode>(fromSocketRef.nodeGuid);
					if (fromNode == null)
					{
						values.Add(default(T));
					}
					else
					{
						var slot = fromNode.FindValueOutputSlot<T>(fromSocketRef.slotId);
						if (slot == null)
						{
							values.Add(default(T));
						}
						else
						{
							values.Add(slot.value);
						}
					}
				}
				ListPool<IEdge>.Release(edges);
			}
		}

		public T GetSlotValue<T>(int inputSlotId)
		{
			var inputSlot = FindSlot<NodeSlot>(inputSlotId);
			if (inputSlot == null)
				return default(T);

			return GetSlotValue<T>(inputSlot);
		}

		public static bool ImplicitConversionExists(SerializedType from, SerializedType to)
		{
			return from.Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == to.Type)
				.Any(mi => {
					ParameterInfo pi = mi.GetParameters().FirstOrDefault();
					return pi != null && pi.ParameterType == from.Type;
				});
		}

		public virtual void ValidateNode()
		{
			var isInError = false;

			// all children nodes needs to be updated first
			// so do that here
			var slots = ListPool<ISlot>.Get();
			GetInputSlots(slots);
			foreach (var inputSlot in slots)
			{
				foreach (var edge in owner.GetEdges(inputSlot.slotReference))
				{
					var fromSocketRef = edge.outputSlot;
					var outputNode = owner.GetNodeFromGuid(fromSocketRef.nodeGuid);
					if (outputNode == null)
						continue;

					outputNode.ValidateNode();
					if (outputNode.hasError)
						isInError = true;
				}
			}

			ListPool<ISlot>.Release(slots);

			// iterate the input slots
			s_TempSlots.Clear();
			GetInputSlots(s_TempSlots);
			foreach (var inputSlot in s_TempSlots)
			{
				s_TempEdges.Clear();
				owner.GetEdges(inputSlot.slotReference,s_TempEdges);

				// if there is a connection
				if (!s_TempEdges.Any())
				{
					continue;
				}

				// get the output details
				var outputSlotRef = s_TempEdges[0].outputSlot;
				var outputNode = owner.GetNodeFromGuid(outputSlotRef.nodeGuid);
				if (outputNode == null)
					continue;

				var outputSlot = outputNode.FindOutputSlot<NodeSlot>(outputSlotRef.slotId);
				if (outputSlot == null)
					continue;

				if (outputSlot.hasError)
				{
					inputSlot.hasError = true;
					continue;
				}

				var outputConcreteType = outputSlot.valueType;

				// if we have a standard connection... just check the types work!
				if (!ImplicitConversionExists(outputConcreteType, inputSlot.valueType))
					inputSlot.hasError = true;
			}

			s_TempSlots.Clear();
			GetInputSlots(s_TempSlots);
			var inputError = s_TempSlots.Any(x => x.hasError);

			// configure the output slots now
			// their slotType will either be the default output slotType
			// or the above dynanic slotType for dynamic nodes
			// or error if there is an input error
			s_TempSlots.Clear();
			GetOutputSlots(s_TempSlots);
			foreach (var outputSlot in s_TempSlots)
			{
				outputSlot.hasError = false;

				if (inputError)
				{
					outputSlot.hasError = true;
					continue;
				}
			}

			// configure the output slots now
			// their slotType will either be the default output slotType
			// or the above dynanic slotType for dynamic nodes
			// or error if there is an input error
			s_TempSlots.Clear();
			GetOutputSlots(s_TempSlots);
			s_TempSlots.Clear();
			GetOutputSlots(s_TempSlots);
			isInError |= CalculateNodeHasError();
			hasError = isInError;

			if (!hasError)
			{
				++version;
			}
		}

		public int version { get; set; }

		//True if error
		protected virtual bool CalculateNodeHasError()
		{
			return false;
		}

		public virtual void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
		{
			s_TempSlots.Clear();
			GetInputSlots(s_TempSlots);
			foreach (var s in s_TempSlots)
			{
				s_TempPreviewProperties.Clear();
				s_TempEdges.Clear();
				owner.GetEdges(s.slotReference, s_TempEdges);
				if (s_TempEdges.Any())
					continue;

				s.GetPreviewProperties(s_TempPreviewProperties);
				for (int i = 0; i < s_TempPreviewProperties.Count; i++)
				{
					if (s_TempPreviewProperties[i].name == null)
						continue;

					properties.Add(s_TempPreviewProperties[i]);
				}
			}
		}

		public virtual string GetVariableNameForNode()
		{
			return defaultVariableName;
		}

		public void AddSlot(ISlot slot)
		{
			if(slot == null) throw new ArgumentNullException("slot");

			// this will remove the old slot and add a new one
			if (m_Slots.Remove(slot.id))
			{
				if(slot is IDisposable) ((IDisposable)slot).Dispose();
			}
			m_Slots.Add(slot.id, slot);
			slot.owner = this;

			Dirty(ModificationScope.Topological);
		}

		public void RemoveSlotsNameNotMatching(IEnumerable<int> index)
		{
			var keys = m_Slots.Keys.Except(index).ToArray();
			foreach (var key in keys)
			{
				m_Slots.Remove(key);
			}
		}

		public void RemoveSlot(int slotId)
		{
			// Remove edges that use this slot
			// no owner can happen after creation
			// but before added to graph
			if (owner != null)
			{
				foreach (var edge in owner.GetEdges(GetSlotReference(slotId)))
					owner.RemoveEdge(edge);
			}

			ISlot slot;
			if (m_Slots.TryGetValue(slotId,out slot))
			{
				if(slot is IDisposable) ((IDisposable)slot).Dispose();
			}
			//remove slots
			m_Slots.Remove(slotId);

			Dirty(ModificationScope.Topological);
		}

		public SlotReference GetSlotReference(int slotId)
		{
			var slot = FindSlot<ISlot>(slotId);
			if (slot == null)
				throw new ArgumentException("Slot could not be found", "slotId");
			return new SlotReference(guid, slotId);
		}

		public T FindSlot<T>(int slotId) where T : ISlot 
		{
			ISlot slot;
			if (m_Slots.TryGetValue(slotId, out slot) && slot is T)
			{
				return (T)slot;
			}

			return default(T);
		}

		public T FindInputSlot<T>(int slotId) where T : ISlot
		{
			ISlot slot;
			if (m_Slots.TryGetValue(slotId, out slot) && slot.isInputSlot && slot is T)
			{
				return (T)slot;
			}

			return default(T);
		}

		public IHasValue<T> FindValueOutputSlot<T>(int slotId)
		{
			ISlot slot;
			if (m_Slots.TryGetValue(slotId, out slot) && slot.isOutputSlot)
			{
				return slot as IHasValue<T>;
			}

			return null;
		}

		public T FindOutputSlot<T>(int slotId) where T : ISlot
		{
			ISlot slot;
			if (m_Slots.TryGetValue(slotId, out slot) && slot.isOutputSlot && slot is T)
			{
				return (T)slot;
			}

			return default(T);
		}

		public virtual int CompareTo(INode other)
		{
			return m_Priority.CompareTo(other.priority);
		}

		#region Slot Creation Helpers

		public T CreateInputSlot<T>(string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(displayName, SlotType.Input);
		}

		public T CreateInputSlot<T>(string idName,string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(idName, displayName, SlotType.Input);
		}

		public T CreateInputSlot<T>(int id, string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(id, displayName, SlotType.Input);
		}

		public T CreateOutputSlot<T>(string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(displayName, SlotType.Output);
		}

		public T CreateOutputSlot<T>(string idName, string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(idName, displayName, SlotType.Output);
		}

		public T CreateOutputSlot<T>(int id, string displayName) where T : NodeSlot, new()
		{
			return CreateSlot<T>(id,displayName, SlotType.Output);
		}

		public T CreateSlot<T>(string displayName,SlotType type) where T : NodeSlot, new()
		{
			return CreateSlot<T>(displayName, displayName, type);
		}

		public T CreateSlot<T>(string idName, string displayName, SlotType type) where T : NodeSlot, new()
		{
			int id = idName.GetHashCode();
			if(m_Slots.ContainsKey(id)) throw new Exception(string.Format("'{0}' has collision detected with {1}. Use a different name or manually assign id", idName,m_Slots[id].displayName));
			return CreateSlot<T>(id,displayName,type);
		}

		#endregion

		public T CreateSlot<T>(int id, string displayName, SlotType type) where T : NodeSlot, new()
		{
			var slot = new T()
			{
				id = id,
				displayName = displayName,
				slotType = type
			};
			AddSlot(slot);
			return slot;
		}

		public virtual IEnumerable<ISlot> GetInputsWithNoConnection()
		{
			return this.GetInputSlots<ISlot>().Where(x =>
			{
				s_TempEdges.Clear();
				owner.GetEdges(GetSlotReference(x.id), s_TempEdges);
				return !s_TempEdges.Any();
			});
		}

		public virtual void OnBeforeSerialize()
		{
			m_GuidSerialized = m_Guid.ToString();
			m_SerializableSlots = SerializationHelper.Serialize<ISlot>(m_Slots,false);
		}

		public virtual void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(m_GuidSerialized))
				m_Guid = new Guid(m_GuidSerialized);
			else
				m_Guid = Guid.NewGuid();

			if(m_Slots == null) m_Slots = new Dictionary<int, ISlot>();
			SerializationHelper.Deserialize(m_Slots,m_SerializableSlots, GraphUtil.GetLegacyTypeRemapping());

			m_SerializableSlots = null;
			foreach (var s in m_Slots.Values)
				s.owner = this;
			UpdateNodeAfterDeserialization();
		}

		public virtual void UpdateNodeAfterDeserialization()
		{
		}

		public bool IsSlotConnected(int slotId)
		{
			var slot = FindSlot<ISlot>(slotId);
			if (slot == null) return false;
			s_TempEdges.Clear();
			owner.GetEdges(slot.slotReference, s_TempEdges);
			return s_TempEdges.Any();
		}

		public virtual void GetSourceAssetDependencies(List<string> paths)
		{
		}

		public virtual void SetOwner(IGraph graph)
		{
			m_Owner = graph;
		}

		public virtual void Dispose()
		{
			foreach (var disposable in m_Slots.OfType<IDisposable>())
			{
				disposable.Dispose();
			}
		}

		public virtual void OnAdd()
		{

		}

		public virtual void OnRemove()
		{

		}
	}
}