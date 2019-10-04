using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeEditor.Util;
using UnityEngine;

namespace NodeEditor
{
    /// <summary>
    /// Base node class for all nodes.
    /// </summary>
	public class AbstractNode : INode, ISerializationCallbackReceiver, IDisposable
	{
		[NonSerialized] private Guid m_Guid;

		[SerializeField] private string m_GuidSerialized;

		[SerializeField] private string m_Name;

		[SerializeField] private DrawState m_DrawState;

		[NonSerialized] private Dictionary<int, ISlot> m_Slots = new Dictionary<int, ISlot>();

		[NonSerialized] private bool m_HasError;

		[NonSerialized] private IGraph m_Owner;

		[SerializeField] private int m_Priority;

        #region Serialization Fields

        [SerializeField] List<SerializationHelper.JSONSerializedIndexedElement> m_SerializableSlots = new List<SerializationHelper.JSONSerializedIndexedElement>();

        #endregion

        OnNodeModified m_OnModified;

        [SerializeField] bool m_PreviewExpanded = true;

        string m_DefaultVariableName;
        string m_NameForDefaultVariableName;
        Guid m_GuidForDefaultVariableName;

        public AbstractNode()
        {
	        name = GetType().Name;
	        m_DrawState.expanded = true;
	        m_Guid = Guid.NewGuid();
	        version = 0;
        }

        /// <summary>
        /// URL link of the documentation of this node.
        /// </summary>
        public virtual string documentationURL => null;

        /// <summary>
        /// A list of all slots.
        /// </summary>
        protected IEnumerable<ISlot> slots => m_Slots.Values;

        #region Implementation of INode

        /// <inheritdoc/>
        public Identifier tempId { get; set; }

        /// <inheritdoc/>
        public IGraph owner => m_Owner;

        /// <inheritdoc/>
        public void RegisterCallback(OnNodeModified callback)
        {
	        m_OnModified += callback;
        }

        /// <inheritdoc/>
        public void UnregisterCallback(OnNodeModified callback)
        {
	        m_OnModified -= callback;
        }

        /// <inheritdoc/>
        public void Dirty(ModificationScope scope)
        {
	        if (m_OnModified != null)
		        m_OnModified(this, scope);
        }

        /// <inheritdoc/>
        public Guid guid => m_Guid;

        /// <inheritdoc/>
        public string name
        {
	        get => m_Name;
	        set => m_Name = value;
        }

        /// <inheritdoc/>
        public virtual bool canDeleteNode => true;

        /// <inheritdoc/>
        public int priority
        {
	        get => m_Priority;
	        set
	        {
		        if (m_Priority != value)
		        {
			        m_Priority = value;
			        OnPriorityChange();
		        }
	        }
        }

        /// <inheritdoc/>
        public DrawState drawState
        {
	        get => m_DrawState;
	        set
	        {
		        m_DrawState = value;
		        Dirty(ModificationScope.Node);
	        }
        }

        /// <inheritdoc/>
        public virtual bool hasError
        {
	        get => m_HasError;
	        protected set => m_HasError = value;
        }

        /// <inheritdoc/>
        public Guid RewriteGuid()
        {
	        m_Guid = Guid.NewGuid();
	        return m_Guid;
        }

        /// <inheritdoc/>
        public void GetInputSlots<T>(List<T> foundSlots) where T : ISlot
        {
	        foreach (var slot in m_Slots.Values)
	        {
		        if (slot.isInputSlot && slot is T)
			        foundSlots.Add((T)slot);
	        }
        }

        /// <inheritdoc/>
        public void GetOutputSlots<T>(List<T> foundSlots) where T : ISlot
        {
	        foreach (var slot in m_Slots.Values)
	        {
		        if (slot.isOutputSlot && slot is T)
			        foundSlots.Add((T)slot);
	        }
        }

        /// <inheritdoc/>
        public void GetSlots<T>(List<T> foundSlots) where T : ISlot
        {
	        foreach (var slot in m_Slots.Values)
	        {
		        if (slot is T)
			        foundSlots.Add((T)slot);
	        }
        }

        /// <inheritdoc/>
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
            var nodeSlots = ListPool<NodeSlot>.Get();
            var edges = ListPool<IEdge>.Get();
            GetInputSlots(nodeSlots);
            foreach (var inputSlot in nodeSlots)
            {
                edges.Clear();
                owner.GetEdges(inputSlot.slotReference, edges);

                // if there is a connection
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

            ListPool<IEdge>.Release(edges);

            nodeSlots.Clear();
            GetInputSlots(nodeSlots);
            var inputError = nodeSlots.Any(x => x.hasError);

            // configure the output slots now
            // their slotType will either be the default output slotType
            // or the above dynanic slotType for dynamic nodes
            // or error if there is an input error
            nodeSlots.Clear();
            GetOutputSlots(nodeSlots);
            foreach (var outputSlot in nodeSlots)
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
            nodeSlots.Clear();
            GetOutputSlots(nodeSlots);
            nodeSlots.Clear();
            GetOutputSlots(nodeSlots);
            ListPool<NodeSlot>.Release(nodeSlots);
            isInError |= CalculateNodeHasError();
            hasError = isInError;

            if (!hasError)
            {
                ++version;
            }

        }

        /// <inheritdoc/>
        public void AddSlot(ISlot slot)
        {
	        if (slot == null) throw new ArgumentNullException("slot");

	        // this will remove the old slot and add a new one
	        if (m_Slots.Remove(slot.id))
	        {
		        if (slot is IDisposable) ((IDisposable)slot).Dispose();
	        }
	        m_Slots.Add(slot.id, slot);
	        slot.owner = this;

	        Dirty(ModificationScope.Topological);
        }

        /// <inheritdoc/>
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

	        if (m_Slots.TryGetValue(slotId, out var slot))
	        {
		        if (slot is IDisposable) ((IDisposable)slot).Dispose();
	        }
	        //remove slots
	        m_Slots.Remove(slotId);

	        Dirty(ModificationScope.Topological);
        }

        /// <inheritdoc/>
        public SlotReference GetSlotReference(int slotId)
        {
	        var slot = FindSlot<ISlot>(slotId);
	        if (slot == null)
		        throw new ArgumentException("Slot could not be found", "slotId");
	        return new SlotReference(guid, slotId);
        }

        /// <inheritdoc/>
        public T FindSlot<T>(int slotId) where T : ISlot
        {
	        if (m_Slots.TryGetValue(slotId, out var slot) && slot is T)
	        {
		        return (T)slot;
	        }

	        return default;
        }

        /// <inheritdoc/>
        public T FindInputSlot<T>(int slotId) where T : ISlot
        {
	        if (m_Slots.TryGetValue(slotId, out var slot) && slot.isInputSlot && slot is T)
	        {
		        return (T)slot;
	        }

	        return default;
        }

        /// <inheritdoc/>
        public T FindOutputSlot<T>(int slotId) where T : ISlot
        {
	        if (m_Slots.TryGetValue(slotId, out var slot) && slot.isOutputSlot && slot is T)
	        {
		        return (T)slot;
	        }

	        return default;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ISlot> GetInputsWithNoConnection()
        {
	        return this.GetInputSlots<ISlot>().Where(x =>
	        {
		        var edges = ListPool<IEdge>.Get();
		        owner.GetEdges(GetSlotReference(x.id), edges);
		        bool value = !edges.Any();
		        ListPool<IEdge>.Release(edges);
		        return value;
	        });
        }

        /// <inheritdoc/>
        public virtual void UpdateNodeAfterDeserialization()
        {
        }

        #endregion

        private void OnPriorityChange()
		{
            if (owner is AbstractNodeGraph abstractNodeGraph)
			{
				abstractNodeGraph.SortEdges(guid);
			}
			Dirty(ModificationScope.Node);
		}

        /// <summary>
        /// Is the preview for this node expanded.
        /// </summary>
		public bool previewExpanded
		{
			get => m_PreviewExpanded;
            set
			{
				if (previewExpanded == value)
					return;
				m_PreviewExpanded = value;
				Dirty(ModificationScope.Node);
			}
		}

        /// <summary>
        /// Nodes that want to have a preview area can override this and return true
        /// </summary>
        public virtual bool hasPreview => false;

        /// <summary>
        /// The type of preview the node will have.
        /// </summary>
		public virtual PreviewMode previewMode => PreviewMode.Preview2D;

        public virtual bool allowedInSubGraph => true;

        /// <summary>
        /// Should this node be allowed in the main graph. To be created by users.
        /// </summary>
		public virtual bool allowedInMainGraph => true;

		public virtual bool allowedInLayerGraph => true;

		string defaultVariableName
		{
			get
			{
				if (m_NameForDefaultVariableName != name || m_GuidForDefaultVariableName != guid)
				{
					m_DefaultVariableName = $"{name}_{GuidEncoder.Encode(guid)}";
					m_NameForDefaultVariableName = name;
					m_GuidForDefaultVariableName = guid;
				}
				return m_DefaultVariableName;
			}
		}

		/// <summary>
        /// Get a value for a slot.
        /// Usually the value for slots is cached after calculations have been executed.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="inputSlot">The value for what slot.</param>
        /// <returns>
        /// The value of a given type from a given slot.
        /// If no slot is found the default value for a value type will be returned.
        /// </returns>
		public T GetSlotValue<T>(ISlot inputSlot)
		{
            if (inputSlot is NodeSlot nodeSlot)
			{
				var con = nodeSlot.GetSlotConnectionCache().FirstOrDefault();

				if (con != null)
				{
                    if (con is IHasValue<T> hasValueSlot)
						return hasValueSlot.value;

                    if (con is IHasValue hasValueBase)
						return (T) hasValueBase.value;
				}

                if (inputSlot is IHasValue<T> hasValueInput)
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
						return default;

					var slot = fromNode.FindValueOutputSlot<T>(fromSocketRef.slotId);
					if (slot == null)
						return default;

					return slot.value;
				}

                return inputSlot is IHasValue<T> hasValue ? hasValue.value : default;
			}

			return default;
		}

        /// <summary>
        /// Get all values for a slot.
        /// </summary>
        /// <typeparam name="T">Type of values to get.</typeparam>
        /// <param name="inputSlot">The slot used to get values from.</param>
        /// <param name="values">This list will be populated with all the values from a given slot.</param>
		public void GetSlotValues<T>(ISlot inputSlot,IList<T> values)
		{
            if (inputSlot is NodeSlot nodeSlot)
			{
				var cache = nodeSlot.GetSlotConnectionCache();
				foreach (var slot in cache)
				{
                    if (slot is IHasValue<T> hasValueSlot)
					{
						values.Add(hasValueSlot.value);
					}
					else
					{
                        if (slot is IHasValue hasValueBase)
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
						values.Add(default);
					}
					else
					{
						var slot = fromNode.FindValueOutputSlot<T>(fromSocketRef.slotId);
						if (slot == null)
						{
							values.Add(default);
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

        /// <summary>
        /// Same as <see cref="GetSlotValue{T}(NodeEditor.ISlot)"/> but with a slot string id.
        /// </summary>
        public T GetSlotValue<T>(int inputSlotId)
		{
			var inputSlot = FindSlot<NodeSlot>(inputSlotId);
			if (inputSlot == null)
				return default;

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

		/// <summary>
        /// The current version fo the node. Each time a validation is ran successfully this will increase.
        /// </summary>
		public int version { get; set; }

        /// <summary>
        /// True if error
        /// </summary>
        /// <returns></returns>
        protected virtual bool CalculateNodeHasError()
		{
			return false;
		}

		public virtual void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
		{
			var nodeSlots = ListPool<NodeSlot>.Get();
			var edges = ListPool<IEdge>.Get();
			var previewProperty = ListPool<PreviewProperty>.Get();

			GetInputSlots(nodeSlots);
			foreach (var s in nodeSlots)
			{
				previewProperty.Clear();
				edges.Clear();
				owner.GetEdges(s.slotReference, edges);
				if (edges.Any())
					continue;

				s.GetPreviewProperties(previewProperty);
				for (int i = 0; i < previewProperty.Count; i++)
				{
					if (previewProperty[i].name == null)
						continue;

					properties.Add(previewProperty[i]);
				}
			}

			ListPool<NodeSlot>.Release(nodeSlots);
			ListPool<IEdge>.Release(edges);
			ListPool<PreviewProperty>.Release(previewProperty);
		}

		public virtual string GetVariableNameForNode()
		{
			return defaultVariableName;
		}

		/// <summary>
        /// Remove all slots with matching ids.
        /// </summary>
        /// <param name="index"></param>
		public void RemoveSlotsNameNotMatching(IEnumerable<int> index)
		{
			var keys = m_Slots.Keys.Except(index).ToArray();
			foreach (var key in keys)
			{
				m_Slots.Remove(key);
			}
		}

		public IHasValue<T> FindValueOutputSlot<T>(int slotId)
		{
            if (m_Slots.TryGetValue(slotId, out var slot) && slot.isOutputSlot)
			{
				return slot as IHasValue<T>;
			}

			return null;
		}

        #region Implementation of IComparable<in INode>

        public virtual int CompareTo(INode other)
        {
	        return m_Priority.CompareTo(other.priority);
        }

        #endregion

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
			if(m_Slots.ContainsKey(id)) throw new Exception(
                $"'{idName}' has collision detected with {m_Slots[id].displayName}. Use a different name or manually assign id");
			return CreateSlot<T>(id,displayName,type);
		}

		#endregion

        /// <summary>
        /// Create a slot in the current node.
        /// </summary>
        /// <typeparam name="T">Type of slot to create.</typeparam>
        /// <param name="id">The id of the slot.</param>
        /// <param name="displayName">The display name of the slot.</param>
        /// <param name="type">Input or output slot.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Is a slot with given ID connected to any other slots.
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
		public bool IsSlotConnected(int slotId)
		{
			var slot = FindSlot<ISlot>(slotId);
			if (slot == null) return false;
			var edges = ListPool<IEdge>.Get();
			owner.GetEdges(slot.slotReference, edges);
			var value = edges.Any();
			ListPool<IEdge>.Release(edges);
			return value;
		}

		public virtual void GetSourceAssetDependencies(List<string> paths)
		{
		}

		public virtual void SetOwner(IGraph graph)
		{
			m_Owner = graph;
		}

        #region Implementation of IDisposable

        public virtual void Dispose()
        {
	        foreach (var disposable in m_Slots.OfType<IDisposable>())
	        {
		        disposable.Dispose();
	        }
        }

        #endregion

        /// <summary>
        /// Called when a node is added to a graph.
        /// </summary>
        public virtual void OnAdd()
		{

		}

        /// <summary>
        /// Called when a node is removed from a graph.
        /// </summary>
		public virtual void OnRemove()
		{

		}

        #region Implementation of ISerializationCallbackReceiver

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

	        if (m_Slots == null) m_Slots = new Dictionary<int, ISlot>();
	        SerializationHelper.Deserialize(m_Slots, m_SerializableSlots, GraphUtil.GetLegacyTypeRemapping());

	        m_SerializableSlots = null;
	        foreach (var s in m_Slots.Values)
		        s.owner = this;
	        UpdateNodeAfterDeserialization();
        }

        #endregion
    }
}