using System;
using SimpleJSON;
using UnityEngine;

namespace NodeEditor
{
	public class GraphObject : GraphObjectBase, ISerializationCallbackReceiver
	{
		[SerializeField,HideInInspector]
		SerializationHelper.JSONSerializedElement m_SerializedGraph;

		[SerializeField, HideInInspector]
		private string m_SerializedGraphData;

		[SerializeField, HideInInspector]
		private int m_SerializationType;

        IGraph m_DeserializedGraph;

		public virtual void OnBeforeSerialize()
		{
			if (graph != null)
			{
				if (m_SerializationType <= 0)
				{
					m_SerializedGraph = SerializationHelper.Serialize(graph);
					m_SerializationType = 0;
				}
				else
				{
					m_SerializedGraphData = JSONExtensions.SerializeFromReflection(graph).ToString();
					m_SerializationType = 1;

				}
			}
		}

		public virtual void OnAfterDeserialize()
		{
			try
			{
				if (m_SerializationType <= 0)
				{
					var deserializedGraph = SerializationHelper.Deserialize<IGraph>(m_SerializedGraph, null);
					if (graph == null)
						graph = deserializedGraph;
					else
						m_DeserializedGraph = deserializedGraph;

					//upgrade serialization
					m_SerializationType = 1;
					m_SerializedGraph = new SerializationHelper.JSONSerializedElement();
				}
				else
				{
					var jsonData = JSONNode.Parse(m_SerializedGraphData);
					var deserializedGraph = (IGraph)jsonData.FromReflection(jsonData["$type"]);

                    if (graph == null)
						graph = deserializedGraph;
					else
						m_DeserializedGraph = deserializedGraph;

                    m_SerializationType = 1;
				}
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