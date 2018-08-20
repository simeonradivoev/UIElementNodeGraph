using System;

namespace NodeEditor.Editor.Controls
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ControlElementAttribute : Attribute
	{
		public Type ControlType;

		public ControlElementAttribute(Type controlType)
		{
			ControlType = controlType;
		}
	}
}