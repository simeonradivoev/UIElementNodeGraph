using System;

namespace NodeEditor
{
	public class GetterSlot<T> : NodeSlot, IHasValue<T>, IHasValue
	{
		private Func<T> m_GetterFunc;

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}

		public GetterSlot<T> SetGetter(Func<T> getter)
		{
			m_GetterFunc = getter;
			return this;
		}

		object IHasValue.value => m_GetterFunc.Invoke();

		public T value => m_GetterFunc.Invoke();

		public override SerializedType valueType => typeof(T);
	}
}