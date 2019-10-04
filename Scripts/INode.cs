using System;
using System.Collections.Generic;

namespace NodeEditor
{
	/// <summary>
    /// Type of node modification.
    /// </summary>
	public enum ModificationScope
	{
		Nothing = 0,
		Node = 1,
		Graph = 2,
		Topological = 3
	}

	public delegate void OnNodeModified(INode node, ModificationScope scope);

	/// <summary>
    /// Base interface for all nodes in a graph.
    /// </summary>
	public interface INode : IComparable<INode>
	{
		void RegisterCallback(OnNodeModified callback);

		void UnregisterCallback(OnNodeModified callback);

		void Dirty(ModificationScope scope);

		/// <summary>
		/// The graph that owns this node.
		/// </summary>
        IGraph owner { get;}

		void SetOwner(IGraph graph);

		/// <summary>
		/// A unique guid for each node. Used to distinguish nodes, reference them and serialize them.
		/// </summary>
        Guid guid { get; }

		/// <summary>
		/// A temporary identifier used to quickly reference nodes.
		/// This id is not serialized therefor is not persistent.
		/// </summary>
        Identifier tempId { get; set; }

		/// <summary>
		/// Rewrites the guid of the node.
		/// </summary>
		/// <returns></returns>
        Guid RewriteGuid();

		/// <summary>
		/// Name of the node. This name can be edited by users.
		/// </summary>
        string name { get; set; }

		int priority { get; set; }

		/// <summary>
		/// Can a node be deleted by the user from a context menu.
		/// </summary>
        bool canDeleteNode { get; }

		/// <summary>
		/// Get all input slots of a given type.
		/// </summary>
		/// <typeparam name="T">The type of slots to search for.</typeparam>
		/// <param name="foundSlots">This list will be populated with the found slots.</param>
        void GetInputSlots<T>(List<T> foundSlots) where T : ISlot;

		/// <summary>
		/// Get all output slots of a given type.
		/// </summary>
		/// <typeparam name="T">The type of slots to search for.</typeparam>
		/// <param name="foundSlots">This list will be populated with the found slots.</param>
        void GetOutputSlots<T>(List<T> foundSlots) where T : ISlot;

		/// <summary>
		/// Get all slots of a given type. This includes input and output slots.
		/// </summary>
		/// <typeparam name="T">The type of slots to search for.</typeparam>
		/// <param name="foundSlots">This list will be populated with the founds slots.</param>
        void GetSlots<T>(List<T> foundSlots) where T : ISlot;

		/// <summary>
		/// Add a slot to the node.
		/// </summary>
		/// <param name="slot">The slot to add.</param>
        void AddSlot(ISlot slot);

		/// <summary>
		/// Remove a slot with a given id.
		/// </summary>
		/// <param name="slotId"></param>
        void RemoveSlot(int slotId);

		/// <summary>
		/// Get a slot reference from a slot ID.
		/// </summary>
		/// <param name="slotId"></param>
		/// <returns></returns>
        SlotReference GetSlotReference(int slotId);

		/// <summary>
		/// Find a slot with a given ID.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="slotId"></param>
		/// <returns></returns>
        T FindSlot<T>(int slotId) where T : ISlot;

		/// <summary>
		/// Find an input slot with a given ID.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="slotId"></param>
		/// <returns></returns>
        T FindInputSlot<T>(int slotId) where T : ISlot;

		T FindOutputSlot<T>(int slotId) where T : ISlot;

		IEnumerable<ISlot> GetInputsWithNoConnection();

		DrawState drawState { get; set; }

		/// <summary>
		/// Does the node currently have an error.
		/// </summary>
        bool hasError { get; }

		/// <summary>
		/// Validate a given node. If there was an error the parameter <see cref="hasError"/> will be set to true.
		/// </summary>
        void ValidateNode();

		void UpdateNodeAfterDeserialization();
	}

	public static class NodeExtensions
	{
		public static IEnumerable<T> GetSlots<T>(this INode node) where T : ISlot
		{
			var slots = new List<T>();
			node.GetSlots(slots);
			return slots;
		}

		public static IEnumerable<T> GetInputSlots<T>(this INode node) where T : ISlot
		{
			var slots = new List<T>();
			node.GetInputSlots(slots);
			return slots;
		}

		public static IEnumerable<T> GetOutputSlots<T>(this INode node) where T : ISlot
		{
			var slots = new List<T>();
			node.GetOutputSlots(slots);
			return slots;
		}
	}
}