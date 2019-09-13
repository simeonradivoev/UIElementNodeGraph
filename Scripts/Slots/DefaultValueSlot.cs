namespace NodeEditor.Slots
{
	public class DefaultValueSlot<T> : NodeSlot, IHasValue<T>, IHasValue
	{
		public override SerializedType valueType => typeof(T);

		private T m_DefaultValue;

		object IHasValue.value => m_DefaultValue;

		public T value
		{
			get => m_DefaultValue;
            private set => m_DefaultValue = value;
        }

		public DefaultValueSlot<T> SetDefaultValue(T val)
		{
			m_DefaultValue = val;
			return this;
		}

		public T this[AbstractNode slot] => slot.GetSlotValue<T>(this);

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}
	}
}