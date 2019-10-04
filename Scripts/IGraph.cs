using System;
using System.Collections.Generic;

namespace NodeEditor
{
	/// <summary>
    /// Base class for all graphs.
    /// </summary>
	public interface IGraph : IOnAssetEnabled
	{
		/// <summary>
        /// Event called when a node is added.
        /// </summary>
		event Action<INode> onNodeAdded;

		/// <summary>
        /// Get all nodes in a graph.
        /// </summary>
        /// <returns></returns>
		ReadOnlyList<INode> GetNodes();

		/// <summary>
        /// Get all edges in a graph.
        /// </summary>
        /// <returns></returns>
		ReadOnlyList<IEdge> GetEdges();

		/// <summary>
        /// Get all edges for a node with given ID.
        /// </summary>
        /// <param name="nodeId">The node all edges belong to.</param>
        /// <returns></returns>
		ReadOnlyList<IEdge> GetEdges(Guid nodeId);

		/// <summary>
        /// Get all properties in a graph.
        /// </summary>
        /// <returns></returns>
		ReadOnlyList<INodeProperty> GetProperties();

		/// <summary>
        /// Get a property with the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		INodeProperty GetProperty(Guid id);

		/// <summary>
        /// Add a node to the graph.
        /// </summary>
        /// <param name="node">The node to add.</param>
		void AddNode(INode node);

		/// <summary>
        /// Remove a node from the graph.
        /// </summary>
        /// <param name="node">The node to remove.</param>
		void RemoveNode(INode node);

		/// <summary>
        /// Connect two slots and get the created edge.
        /// </summary>
        /// <param name="fromSlotRef">From what slot to connect from.</param>
        /// <param name="toSlotRef">The slot to connect to.</param>
        /// <returns>The created edge between the two slots.</returns>
		IEdge Connect(SlotReference fromSlotRef, SlotReference toSlotRef);

		/// <summary>
        /// Remove a give edge from the graph.
        /// </summary>
        /// <param name="e">The edge to remove.</param>
		void RemoveEdge(IEdge e);

		/// <summary>
        /// Remove all provided nodes and edges at once.
        /// </summary>
        /// <param name="nodes">All the nodes to remove.</param>
        /// <param name="edges">All the edges to remove.</param>
		void RemoveElements(IEnumerable<INode> nodes, IEnumerable<IEdge> edges);
		
		/// <summary>
        /// Get a node from an ID.
        /// </summary>
        /// <param name="guid">The ID of the node to get.</param>
        /// <returns>The node with the given ID.</returns>
		INode GetNodeFromGuid(Guid guid);

		/// <summary>
        /// Is there a node with a given ID in the graph.
        /// </summary>
        /// <param name="guid">The ID of the node.</param>
        /// <returns>Is there a node with the given ID.</returns>
		bool ContainsNodeGuid(Guid guid);

		T GetNodeFromGuid<T>(Guid guid) where T : INode;

		/// <summary>
        /// Validate the graph for any errors.
        /// </summary>
		void ValidateGraph();

		/// <summary>
        /// Replace this graph with another.
        /// </summary>
        /// <param name="other">The graph to replace with.</param>
		void ReplaceWith(IGraph other);

		/// <summary>
        /// The object owner for the graph.
        /// Could be null if the graph was stored directly on disk.
        /// </summary>
		IGraphObject owner { get; set; }

		/// <summary>
        /// All added nodes so far.
        /// </summary>
		IEnumerable<INode> addedNodes { get; }

		/// <summary>
        /// All removed nodes so far.
        /// </summary>
		IEnumerable<INode> removedNodes { get; }

		/// <summary>
        /// All added edges so far.
        /// </summary>
		IEnumerable<IEdge> addedEdges { get; }

		/// <summary>
        /// All removed edges so far.
        /// </summary>
		IEnumerable<IEdge> removedEdges { get; }

		/// <summary>
        /// Clear all changes.
        /// This clears all removed and added edges and graphs.
        /// </summary>
		void ClearChanges();
	}
}