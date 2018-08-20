using System;

namespace NodeEditor.Editor.Tests
{
	public class TestSlot : NodeSlot
	{
		public TestSlot()
		{
		}

		public TestSlot(int slotId, string displayName, Type valueType, SlotType slotType, bool hidden = false) : base(slotId, displayName, valueType, slotType, hidden)
		{
		}

		public TestSlot(int slotId, string displayName, Type valueType, SlotType slotType, int priority, bool hidden = false) : base(slotId, displayName, valueType, slotType, priority, hidden)
		{
		}

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}

		public override T GetValue<T>()
		{
			return default(T);
		}
	}
}