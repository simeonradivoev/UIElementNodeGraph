using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeEditor.Util;
using UnityEngine;

namespace NodeEditor
{
	public class AbstractNode : INode, ISerializationCallbackReceiver
	{
		protected static List<NodeSlot> s_TempSlots = new List<NodeSlot>();
		protected static List<IEdge> s_TempEdges = new List<IEdge>();
		protected static List<PreviewProperty> s_TempPreviewProperties = new List<PreviewProperty>();

		[NonSerialized] private Guid m_Guid;

		[SerializeField] private string m_GuidSerialized;

		[SerializeField] private string m_Name;

		[SerializeField] private DrawState m_DrawState;

		[NonSerialized] private List<ISlot> m_Slots = new List<ISlot>();

		[SerializeField] List<SerializationHelper.JSONSerializedElement> m_SerializableSlots = new List<SerializationHelper.JSONSerializedElement>();

		[NonSerialized] private bool m_HasError;

		public Identifier tempId { get; set; }

		public IGraph owner { get; set; }

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

		public Guid guid
		{
			get { return m_Guid; }
		}

		public string name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		public virtual string documentationURL
		{
			get { return null; }
		}

		public virtual bool canDeleteNode
		{
			get { return true; }
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
		public virtual bool hasPreview
		{
			get { return false; }
		}

		public virtual PreviewMode previewMode
		{
			get { return PreviewMode.Preview2D; }
		}

		// Nodes that want to have a preview area can override this and return true

		public virtual bool allowedInSubGraph
		{
			get { return true; }
		}

		public virtual bool allowedInMainGraph
		{
			get { return true; }
		}

		public virtual bool allowedInLayerGraph
		{
			get { return true; }
		}

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

		protected AbstractNode()
		{
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
			foreach (var slot in m_Slots)
			{
				if (slot.isInputSlot && slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public void GetOutputSlots<T>(List<T> foundSlots) where T : ISlot
		{
			foreach (var slot in m_Slots)
			{
				if (slot.isOutputSlot && slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public void GetSlots<T>(List<T> foundSlots) where T : ISlot
		{
			foreach (var slot in m_Slots)
			{
				if (slot is T)
					foundSlots.Add((T) slot);
			}
		}

		public T GetSlotValue<T>(int inputSlotId)
		{
			var inputSlot = FindSlot<NodeSlot>(inputSlotId);
			if (inputSlot == null)
				return default(T);

			var edge = owner.GetEdges(inputSlot.slotReference).FirstOrDefault();

			if (edge != null)
			{
				var fromSocketRef = edge.outputSlot;
				var fromNode = owner.GetNodeFromGuid<AbstractNode>(fromSocketRef.nodeGuid);
				if (fromNode == null)
					return default(T);

				var slot = fromNode.FindOutputSlot<NodeSlot>(fromSocketRef.slotId);
				if (slot == null)
					return default(T);

				//todo find a way to get value
				return slot.GetValue<T>();
			}

			return inputSlot.GetValue<T>();
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
				var edges = owner.GetEdges(inputSlot.slotReference);
				foreach (var edge in edges)
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
				// if there is a connection
				var edges = owner.GetEdges(inputSlot.slotReference).ToList();
				if (!edges.Any())
				{
					continue;
				}

				// get the output details
				var outputSlotRef = edges[0].outputSlot;
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
			m_Slots.RemoveAll(x => x.id == slot.id);
			m_Slots.Add(slot);
			slot.owner = this;

			Dirty(ModificationScope.Topological);
		}

		public void RemoveSlot(int slotId)
		{
			// Remove edges that use this slot
			// no owner can happen after creation
			// but before added to graph
			if (owner != null)
			{
				var edges = owner.GetEdges(GetSlotReference(slotId));

				foreach (var edge in edges.ToArray())
					owner.RemoveEdge(edge);
			}

			//remove slots
			m_Slots.RemoveAll(x => x.id == slotId);

			Dirty(ModificationScope.Topological);
		}

		public void RemoveSlotsNameNotMatching(IEnumerable<int> slotIds, bool supressWarnings = false)
		{
			var invalidSlots = m_Slots.Select(x => x.id).Except(slotIds);

			foreach (var invalidSlot in invalidSlots.ToArray())
			{
				if (!supressWarnings)
					Debug.LogWarningFormat("Removing Invalid Slot: {0}", invalidSlot);
				RemoveSlot(invalidSlot);
			}
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
			foreach (var slot in m_Slots)
			{
				if (slot.id == slotId && slot is T)
					return (T) slot;
			}

			return default(T);
		}

		public T FindInputSlot<T>(int slotId) where T : ISlot
		{
			foreach (var slot in m_Slots)
			{
				if (slot.isInputSlot && slot.id == slotId && slot is T)
					return (T) slot;
			}

			return default(T);
		}

		public T FindOutputSlot<T>(int slotId) where T : ISlot
		{
			foreach (var slot in m_Slots)
			{
				if (slot.isOutputSlot && slot.id == slotId && slot is T)
					return (T) slot;
			}

			return default(T);
		}

		public virtual IEnumerable<ISlot> GetInputsWithNoConnection()
		{
			return this.GetInputSlots<ISlot>().Where(x => !owner.GetEdges(GetSlotReference(x.id)).Any());
		}

		public virtual void OnBeforeSerialize()
		{
			m_GuidSerialized = m_Guid.ToString();
			m_SerializableSlots = SerializationHelper.Serialize<ISlot>(m_Slots);
		}

		public virtual void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(m_GuidSerialized))
				m_Guid = new Guid(m_GuidSerialized);
			else
				m_Guid = Guid.NewGuid();

			m_Slots = SerializationHelper.Deserialize<ISlot>(m_SerializableSlots, GraphUtil.GetLegacyTypeRemapping());
			m_SerializableSlots = null;
			foreach (var s in m_Slots)
				s.owner = this;
			UpdateNodeAfterDeserialization();
		}

		public virtual void UpdateNodeAfterDeserialization()
		{
		}

		public bool IsSlotConnected(int slotId)
		{
			var slot = FindSlot<ISlot>(slotId);
			return slot != null && owner.GetEdges(slot.slotReference).Any();
		}

		public virtual void GetSourceAssetDependencies(List<string> paths)
		{
		}
	}
}