using System;
using UnityEngine;

namespace NodeEditor
{
	public abstract class GraphObjectBase : ScriptableObject, IGraphObject
	{
		public virtual Type GraphType => typeof(NodeGraph);
		private bool m_IsEnabled;

		IGraph m_Graph;

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

		private void OnNodeAdded(INode node)
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

		public virtual bool isDirty => false;

        void ValidateInternal()
		{
			if (graph != null)
			{
				graph.OnEnable();
				graph.ValidateGraph();
				graph.onNodeAdded += OnNodeAdded;
			}

			Validate();
		}

		void OnEnable()
		{
			m_IsEnabled = true;
			ValidateInternal();

#if UNITY_EDITOR
			UnityEditor.Undo.undoRedoPerformed += UndoRedoPerformed;
#endif
			UndoRedoPerformed();
		}

		void OnDisable()
		{
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