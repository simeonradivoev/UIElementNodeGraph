using System;
using UnityEngine;

namespace NodeEditor
{
	public abstract class AbstractNodeProperty<T> : INodeProperty
	{
		[SerializeField]
		private T m_Value;

		[SerializeField]
		private string m_Name;

		[SerializeField]
		private bool m_Exposed = true;

		[SerializeField]
		private SerializableGuid m_Guid = new SerializableGuid();

		public T value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		public string displayName
		{
			get
			{
				if (string.IsNullOrEmpty(m_Name))
					return guid.ToString();
				return m_Name;
			}
			set { m_Name = value; }
		}

		string m_DefaultReferenceName;

		public string referenceName
		{
			get
			{
				if (string.IsNullOrEmpty(overrideReferenceName))
				{
					if (string.IsNullOrEmpty(m_DefaultReferenceName))
						m_DefaultReferenceName = string.Format("{0}_{1}", propertyType, GuidEncoder.Encode(guid));
					return m_DefaultReferenceName;
				}
				return overrideReferenceName;
			}
		}

		[SerializeField]
		string m_OverrideReferenceName;

		public string overrideReferenceName
		{
			get { return m_OverrideReferenceName; }
			set { m_OverrideReferenceName = value; }
		}

		public abstract SerializedType propertyType { get; }

		public Guid guid
		{
			get { return m_Guid.guid; }
		}

		public bool exposed
		{
			get { return m_Exposed;}
			set { m_Exposed = value; }
		}

		public abstract object defaultValue { get; }
		public abstract PreviewProperty GetPreviewNodeProperty();
		public abstract INode ToConcreteNode();
		public abstract INodeProperty Copy();
	}
}