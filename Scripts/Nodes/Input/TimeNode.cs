using NodeEditor.Slots;
using UINodeEditor;
using UnityEngine;

namespace NodeEditor.Nodes.Input
{
	[Title("Input","Time")]
	public class TimeNode : AbstractNode, ITickableNode
	{
		private DefaultValueSlot<float> m_Time;
		private DefaultValueSlot<float> m_UnscaledTime;
		private DefaultValueSlot<float> m_DeltaTime;
		private DefaultValueSlot<float> m_UnscaledDeltaTime;

		public TimeNode()
		{
			m_Time = CreateOutputSlot<DefaultValueSlot<float>>("Time");
			m_UnscaledTime = CreateOutputSlot<DefaultValueSlot<float>>("Unscaled Time");
			m_DeltaTime = CreateOutputSlot<DefaultValueSlot<float>>("Delta Time");
			m_UnscaledDeltaTime = CreateOutputSlot<DefaultValueSlot<float>>("Unscaled Delta Time");
		}

		public void Tick()
		{
			m_Time.SetDefaultValue(Time.time);
			m_UnscaledTime.SetDefaultValue(Time.unscaledTime);
			m_DeltaTime.SetDefaultValue(Time.deltaTime);
			m_UnscaledDeltaTime.SetDefaultValue(Time.unscaledDeltaTime);
		}
	}
}