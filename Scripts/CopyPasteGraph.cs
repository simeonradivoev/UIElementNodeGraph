using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditor.Util;
using UnityEngine;

namespace NodeEditor
{
	[Serializable]
	public class CopyPasteGraph : ISerializationCallbackReceiver
	{
		[NonSerialized]
		HashSet<IEdge> m_Edges = new HashSet<IEdge>();

		[NonSerialized]
		HashSet<INode> m_Nodes = new HashSet<INode>();

		[NonSerialized]
		HashSet<INodeProperty> m_Properties = new HashSet<INodeProperty>();

		// The meta properties are properties that are not copied into the tatget graph
		// but sent along to allow property nodes to still hvae the data from the original
		// property present.
		[NonSerialized]
		HashSet<INodeProperty> m_MetaProperties = new HashSet<INodeProperty>();

		[NonSerialized]
		SerializableGuid m_SourceGraphGuid;

		[SerializeField]
		List<SerializationHelper.JSONSerializedElement> m_SerializableNodes = new List<SerializationHelper.JSONSerializedElement>();

		[SerializeField]
		List<SerializationHelper.JSONSerializedElement> m_SerializableEdges = new List<SerializationHelper.JSONSerializedElement>();

		[SerializeField]
		List<SerializationHelper.JSONSerializedElement> m_SerilaizeableProperties = new List<SerializationHelper.JSONSerializedElement>();

		[SerializeField]
		List<SerializationHelper.JSONSerializedElement> m_SerializableMetaProperties = new List<SerializationHelper.JSONSerializedElement>();

		[SerializeField]
		SerializationHelper.JSONSerializedElement m_SerializeableSourceGraphGuid = new SerializationHelper.JSONSerializedElement();

		public CopyPasteGraph() {}

		public CopyPasteGraph(Guid sourceGraphGuid, IEnumerable<INode> nodes, IEnumerable<IEdge> edges, IEnumerable<INodeProperty> properties, IEnumerable<INodeProperty> metaProperties)
		{
			m_SourceGraphGuid = new SerializableGuid(sourceGraphGuid);

			foreach (var node in nodes)
			{
				AddNode(node);
				foreach (var edge in NodeUtils.GetAllEdges(node))
					AddEdge(edge);
			}

			foreach (var edge in edges)
				AddEdge(edge);

			foreach (var property in properties)
				AddProperty(property);

			foreach (var metaProperty in metaProperties)
				AddMetaProperty(metaProperty);
		}

		public void AddNode(INode node)
		{
			m_Nodes.Add(node);
		}

		public void AddEdge(IEdge edge)
		{
			m_Edges.Add(edge);
		}

		public void AddProperty(INodeProperty property)
		{
			m_Properties.Add(property);
		}

		public void AddMetaProperty(INodeProperty metaProperty)
		{
			m_MetaProperties.Add(metaProperty);
		}

		public IEnumerable<T> GetNodes<T>() where T : INode
		{
			return m_Nodes.OfType<T>();
		}

		public IEnumerable<IEdge> edges
		{
			get { return m_Edges; }
		}

		public IEnumerable<INodeProperty> properties
		{
			get { return m_Properties; }
		}

		public IEnumerable<INodeProperty> metaProperties
		{
			get { return m_MetaProperties; }
		}

		public Guid sourceGraphGuid
		{
			get { return m_SourceGraphGuid.guid; }
		}

		public void OnBeforeSerialize()
		{
			m_SerializeableSourceGraphGuid = SerializationHelper.Serialize(m_SourceGraphGuid);
			m_SerializableNodes = SerializationHelper.Serialize<INode>(m_Nodes);
			m_SerializableEdges = SerializationHelper.Serialize<IEdge>(m_Edges);
			m_SerilaizeableProperties = SerializationHelper.Serialize<INodeProperty>(m_Properties);
			m_SerializableMetaProperties = SerializationHelper.Serialize<INodeProperty>(m_MetaProperties);
		}

		public void OnAfterDeserialize()
		{
			m_SourceGraphGuid = SerializationHelper.Deserialize<SerializableGuid>(m_SerializeableSourceGraphGuid, GraphUtil.GetLegacyTypeRemapping());

			var nodes = SerializationHelper.Deserialize<INode>(m_SerializableNodes, GraphUtil.GetLegacyTypeRemapping());
			m_Nodes.Clear();
			foreach (var node in nodes)
				m_Nodes.Add(node);
			m_SerializableNodes = null;

			var edges = SerializationHelper.Deserialize<IEdge>(m_SerializableEdges, GraphUtil.GetLegacyTypeRemapping());
			m_Edges.Clear();
			foreach (var edge in edges)
				m_Edges.Add(edge);
			m_SerializableEdges = null;

			var properties = SerializationHelper.Deserialize<INodeProperty>(m_SerilaizeableProperties, GraphUtil.GetLegacyTypeRemapping());
			m_Properties.Clear();
			foreach (var property in properties)
				m_Properties.Add(property);
			m_SerilaizeableProperties = null;

			var metaProperties = SerializationHelper.Deserialize<INodeProperty>(m_SerializableMetaProperties, GraphUtil.GetLegacyTypeRemapping());
			m_MetaProperties.Clear();
			foreach (var metaProperty in metaProperties)
			{
				m_MetaProperties.Add(metaProperty);
			}
			m_SerializableMetaProperties = null;
		}

		public static CopyPasteGraph FromJson(string copyBuffer)
		{
			try
			{
				return JsonUtility.FromJson<CopyPasteGraph>(copyBuffer);
			}
			catch
			{
				// ignored. just means copy buffer was not a graph :(
				return null;
			}
		}
	}
}