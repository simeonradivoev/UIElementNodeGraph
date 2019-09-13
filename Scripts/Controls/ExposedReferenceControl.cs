using NodeEditor.Controls.Views;
using UnityEngine.UIElements;

namespace NodeEditor.Controls
{
	public class ExposedReferenceControl : ControlAttribute
	{
		public override VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property)
		{
			return new ExposedReferenceControlView(node,property);
		}
	}
}