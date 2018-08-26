using System;
using System.Linq;
using NodeEditor.Controls.Views;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	[Serializable]
	public class ValueSlot<T> : NodeSlot, IHasValue<T>, IHasValue, IValueSetter<T>
	{
		private static Type[] SuportedTypes =
		{
			typeof(bool),
			typeof(float),
			typeof(double),
			typeof(string),
			typeof(int),
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Quaternion),
			typeof(Color),
			typeof(Rect),
			typeof(Gradient),
			typeof(AnimationCurve)
		};

		[SerializeField] private T m_Value;
		private bool m_ShowControl;

		public ValueSlot<T> SetShowControl()
		{
			m_ShowControl = true;
			return this;
		}

		object IHasValue.value => m_Value;

		public T value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		void IValueSetter<T>.SetValue(T val)
		{
			m_Value = val;
		}

		public ValueSlot<T> SetValue(T val)
		{
			m_Value = val;
			return this;
		}

		public override VisualElement InstantiateControl()
		{
			if (m_ShowControl && SuportedTypes.Contains(typeof(T)))
			{
				return new ValueSlotControlView<T>(this);
			}
			return base.InstantiateControl();
		}

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
		}

		public T this[AbstractNode slot] => slot.GetSlotValue<T>(this);

		public override SerializedType valueType => typeof(T);
	}
}