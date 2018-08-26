using NodeEditor.Slots;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Controls.Views
{
	public class SliderSlotControlView : VisualElement
	{
		public SliderSlotControlView(SliderSlot slot)
		{
			AddStyleSheetPath("Styles/Controls/SliderControlView");
			var field = new Slider()
			{
				value = slot.value,
				lowValue = slot.min,
				highValue = slot.max
			};
			field.valueChanged += v =>
			{
				slot.owner.owner.owner.RegisterCompleteObjectUndo("Slider Change");
				slot.value = v; 
				slot.owner.Dirty(ModificationScope.Node);
			};
			Add(field);
		}
	}
}