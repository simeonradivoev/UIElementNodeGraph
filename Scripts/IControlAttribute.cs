using NodeEditor.Controls;
using UnityEngine.UIElements;

namespace NodeEditor
{
	public interface IControlAttribute
	{
		VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property);
	}
}