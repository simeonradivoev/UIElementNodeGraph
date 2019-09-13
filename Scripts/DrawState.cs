using System;
using UnityEngine;

namespace NodeEditor
{
	[Serializable]
	public struct DrawState
	{
		[SerializeField]
		private bool m_Expanded;

		[SerializeField]
		private Rect m_Position;

		public bool expanded
		{
			get => m_Expanded;
            set => m_Expanded = value;
        }

		public Rect position
		{
			get => m_Position;
            set => m_Position = value;
        }
	}
}