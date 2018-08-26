using NodeEditor.Controls.Views;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Slots
{
	public class LayerMaskSlot : ValueSlot<int>
	{
		public override VisualElement InstantiateControl()
		{
			return new LayerMaskSlotControlView(this);
		}
	}
}