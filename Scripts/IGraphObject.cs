namespace NodeEditor
{
	/// <summary>
    /// Base interface for all objects that hold a graph.
    /// Mainly used for scriptable objects that hold a single graph.
    /// </summary>
	public interface IGraphObject
	{
		/// <summary>
        /// The owned graph.
        /// </summary>
		IGraph graph { get; set; }
		void RegisterCompleteObjectUndo(string name);
	}
}