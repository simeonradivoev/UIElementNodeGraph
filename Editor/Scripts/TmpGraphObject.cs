using UnityEngine;

namespace NodeEditor
{
	internal class TmpGraphObject : GraphObject
	{
		[SerializeField, HideInInspector]
		bool m_IsDirty;

		public override void SetDirty(bool dirty)
		{
			m_IsDirty = dirty;
		}

		public override bool isDirty
		{
			get { return m_IsDirty; }
		}
	}
}