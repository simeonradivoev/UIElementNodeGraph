using NodeEditor.Slots;
using UnityEngine;

namespace NodeEditor.Nodes
{
	[Title("Samplers","Gradient Sampler")]
	public class GradientSamplerNode : AbstractNode
	{
		private ValueSlot<Gradient> m_Gradient;
		private SliderSlot m_Time;

		public GradientSamplerNode()
		{
			m_Gradient = CreateInputSlot<ValueSlot<Gradient>>("In").SetShowControl();
			m_Time = CreateInputSlot<SliderSlot>("Value").SetRange(0,1);
			CreateOutputSlot<GetterSlot<Color>>("Color").SetGetter(SampleColor);
		}

		private Color SampleColor()
		{
			return m_Gradient[this].Evaluate(GetSlotValue<float>(m_Time));
		}
	}
}