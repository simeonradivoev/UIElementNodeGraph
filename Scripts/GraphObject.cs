using System;
using UnityEngine;

namespace NodeEditor
{
	public class GraphObject : GraphObjectBase, ISerializationCallbackReceiver
	{
		[SerializeField,HideInInspector]
		SerializationHelper.JSONSerializedElement m_SerializedGraph;

		IGraph m_DeserializedGraph;

		public virtual void OnBeforeSerialize()
		{
			if (graph != null)
				m_SerializedGraph = SerializationHelper.Serialize(graph,false);
		}

		public virtual void OnAfterDeserialize()
		{
			try
			{
				var deserializedGraph = SerializationHelper.Deserialize<IGraph>(m_SerializedGraph, null);
				if (graph == null)
					graph = deserializedGraph;
				else
					m_DeserializedGraph = deserializedGraph;
			}
			catch (Exception e)
			{
				// ignored
			}
		}

		protected override void UndoRedoPerformed()
		{
			if (m_DeserializedGraph != null)
			{
				graph.ReplaceWith(m_DeserializedGraph);
				m_DeserializedGraph = null;
			}
		}
	}
}