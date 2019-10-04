using NodeEditor.Controls;
using SimpleJSON;
using UnityEngine;

namespace NodeEditor.Nodes
{
	public class ResourceNode<T> : AbstractNode where T : Object
	{
		[SerializeField] private string m_ResourcePath;
		private T m_CachedResource;

		[DefaultControl]
		public string resourcePath
		{
			get => m_ResourcePath;
            set => m_ResourcePath = value;
        }

		public ResourceNode()
		{
			name = "Resource";
			CreateOutputSlot<GetterSlot<T>>(0, "Out").SetGetter(GetResource);
		}

		private T GetResource()
		{
			if (m_CachedResource == null && string.IsNullOrEmpty(m_ResourcePath))
				m_CachedResource = Resources.Load<T>(m_ResourcePath);
			return m_CachedResource;
		}
	}
}