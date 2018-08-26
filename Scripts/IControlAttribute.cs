using NodeEditor.Controls;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	public interface IControlAttribute
	{
		VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property);
	}
}