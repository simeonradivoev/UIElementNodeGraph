using NodeEditor.Controls.Views;
using UnityEngine.UIElements;

namespace NodeEditor.Slots
{
	public class LayerSlot : ValueSlot<int>
	{
		public override VisualElement InstantiateControl()
		{
			return new LayerFieldSlotControlView(this);
		}
	}
}