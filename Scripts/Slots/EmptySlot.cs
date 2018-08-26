namespace NodeEditor
{
	public class EmptySlot<T> : NodeSlot
	{
		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}

		public T this[AbstractNode slot] => slot.GetSlotValue<T>(this);

		public override SerializedType valueType => typeof(T);
	}
}