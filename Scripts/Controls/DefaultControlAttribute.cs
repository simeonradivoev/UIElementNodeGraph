using NodeEditor.Controls.Views;
using UnityEngine.UIElements;

namespace NodeEditor.Controls
{
	public class DefaultControlAttribute : ControlAttribute
	{
		public override VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property)
		{
			return new DefaultControlView(this,node, property);
		}
	}
}