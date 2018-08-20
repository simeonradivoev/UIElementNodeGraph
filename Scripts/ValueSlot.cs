using System;
using UnityEngine;

namespace NodeEditor
{
	[Serializable]
	public class ValueSlot<T> : NodeSlot, IHasValue<T>, IValueSetter<T>
	{
		[SerializeField] private T m_Value;

		public T value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		public void SetValue(T val)
		{
			m_Value = val;
		}

		public ValueSlot()
		{
		}

		public ValueSlot(int slotId, string displayName, SlotType slotType, bool hidden = false) : base(slotId, displayName, typeof(T), slotType, hidden)
		{
		}

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}

		public override T1 GetValue<T1>()
		{
			return (T1)(object)m_Value;
		}
	}
}