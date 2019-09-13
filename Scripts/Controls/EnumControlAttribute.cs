using NodeEditor.Controls.Views;
using UnityEngine.UIElements;

namespace NodeEditor.Controls
{
	public class EnumControlAttribute : ControlAttribute
	{
		public override VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property)
		{
			return new EnumControlView(this,node,property);
		}
	}
}