using System;

namespace NodeEditor.Editor.Tests
{
	public class TestSlot : NodeSlot
	{
		private SerializedType m_ValueType;

		public TestSlot()
		{
		}

		public TestSlot(SerializedType mValueType,SlotType type,int id)
		{
			m_ValueType = mValueType;
			slotType = type;
			this.id = id;
		}

		public TestSlot SetValueType(Type type)
		{
			m_ValueType = type;
			return this;
		}

		public override SerializedType valueType
		{
			get { return m_ValueType; }
		}

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}
	}
}