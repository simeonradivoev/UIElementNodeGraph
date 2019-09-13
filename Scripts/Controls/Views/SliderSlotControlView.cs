using NodeEditor.Slots;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor.Controls.Views
{
	public class SliderSlotControlView : VisualElement
	{
		public SliderSlotControlView(SliderSlot slot)
		{
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/Controls/SliderControlView"));
			var field = new Slider()
			{
				value = slot.value,
				lowValue = slot.min,
				highValue = slot.max
			};
			field.RegisterValueChangedCallback(e =>
			{
				slot.owner.owner.owner.RegisterCompleteObjectUndo("Slider Change");
				slot.value = e.newValue;
				slot.owner.Dirty(ModificationScope.Node);
			});
			Add(field);
		}
	}
}