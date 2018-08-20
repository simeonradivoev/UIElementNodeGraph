using System.Reflection;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	public interface IControlAttribute
	{
		VisualElement InstantiateControl(AbstractNode node, PropertyInfo propertyInfo);
	}
}