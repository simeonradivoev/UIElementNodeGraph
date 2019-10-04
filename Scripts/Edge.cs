using System;
using UnityEngine;

namespace NodeEditor
{
	[Serializable]
	public class Edge : IEdge, IEquatable<IEdge>
	{
		[SerializeField]
		private SlotReference m_OutputSlot;
		[SerializeField]
		private SlotReference m_InputSlot;

		public Edge()
		{}

		public Edge(SlotReference outputSlot, SlotReference inputSlot)
		{
			m_OutputSlot = outputSlot;
			m_InputSlot = inputSlot;
		}

		public SlotReference outputSlot => m_OutputSlot;

		public SlotReference inputSlot => m_InputSlot;

		protected bool Equals(Edge other)
		{
			return Equals(m_OutputSlot, other.m_OutputSlot) && Equals(m_InputSlot, other.m_InputSlot);
		}

        #region Implementation of IEquatable<IEdge>

        public bool Equals(IEdge other)
        {
	        return Equals(other as object);
        }

        #endregion

        #region Overrides of Object

        public override bool Equals(object obj)
        {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((Edge)obj);
        }

        public override int GetHashCode()
        {
	        unchecked
	        {
		        // Can't make fields readonly due to Unity serialization
		        return (m_OutputSlot.GetHashCode() * 397) ^ m_InputSlot.GetHashCode();
	        }
        }

        #endregion
    }
}