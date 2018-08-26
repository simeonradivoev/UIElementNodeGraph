using System;
using NodeEditor.Controls;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public abstract class ControlAttribute : Attribute, IControlAttribute
	{
		public string label;
		public abstract VisualElement InstantiateControl(AbstractNode node, ReflectionProperty property);
	}
}