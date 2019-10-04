using System;
using UnityEngine;

namespace NodeEditor
{
	public abstract class GraphObjectBase : ScriptableObject, IGraphObject
	{
		public virtual Type GraphType => typeof(NodeGraph);
		private bool m_IsEnabled;

		private IGraph m_Graph;

        #region Implementation of IGraphObject

        public IGraph graph
        {
	        get => m_Graph;
	        set
	        {
		        if (m_Graph != null)
		        {
			        m_Graph.owner = null;
			        m_Graph.onNodeAdded += OnNodeAdded;
		        }

		        m_Graph = value;
		        if (m_Graph != null)
		        {
			        m_Graph.owner = this;
			        m_Graph.onNodeAdded -= OnNodeAdded;
		        }
	        }
        }

        public void RegisterCompleteObjectUndo(string name)
        {
#if UNITY_EDITOR
	        UnityEditor.Undo.RegisterCompleteObjectUndo(this, name);
#endif
        }

        #endregion

        protected void OnNodeAdded(INode node)
		{
			if (!m_IsEnabled && Application.isPlaying) return;
			var onEnableNode = node as IOnAssetEnabled;
			onEnableNode?.OnEnable();
		}

		protected virtual void Validate()
		{

		}

		public virtual void SetDirty(bool dirty)
		{

		}

		protected virtual void OnObjectEnable()
		{

		}

		protected virtual void OnObjectDisable()
		{

		}

		public virtual bool isDirty => false;

		private void ValidateInternal()
		{
			if (graph != null)
			{
				graph.OnEnable();
				graph.ValidateGraph();
				graph.onNodeAdded += OnNodeAdded;
			}

			Validate();
		}

		private void OnEnable()
		{
			m_IsEnabled = true;

			OnObjectEnable();

			ValidateInternal();

#if UNITY_EDITOR
			UnityEditor.Undo.undoRedoPerformed += UndoRedoPerformed;
#endif
			UndoRedoPerformed();
		}

		private void OnDisable()
		{
			OnObjectDisable();

#if UNITY_EDITOR
			UnityEditor.Undo.undoRedoPerformed -= UndoRedoPerformed;
#endif
			(graph as IDisposable)?.Dispose();
			if (graph != null) graph.onNodeAdded -= OnNodeAdded;
		}

		protected virtual void UndoRedoPerformed()
		{

		}
	}
}