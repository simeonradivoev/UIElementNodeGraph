using System;

namespace NodeEditor
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ControlAttribute : Attribute, IControlAttribute
	{
		public string label;
	}
}