using NodeEditor.Controls.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor.Slots
{
	public class SliderSlot : NodeSlot, IHasValue<float>, IHasValue, IValueSetter<float>
	{
		private float m_Min;
		private float m_Max;
		[SerializeField] private float m_Value;

		public float min => m_Min;

		public float max => m_Max;

		object IHasValue.value => m_Value;

		public float value
		{
			get => m_Value;
            set => m_Value = value;
        }

		public SliderSlot SetRange(float min, float max)
		{
			m_Min = min;
			m_Max = max;
			return this;
		}

		void IValueSetter<float>.SetValue(float val)
		{
			m_Value = val;
		}

		public SliderSlot SetValue(float val)
		{
			m_Value = val;
			return this;
		}

		public override SerializedType valueType => typeof(float);

		public override void CopyValuesFrom(NodeSlot foundSlot)
		{
			
		}

		public override VisualElement InstantiateControl()
		{
			return new SliderSlotControlView(this);
		}
	}
}