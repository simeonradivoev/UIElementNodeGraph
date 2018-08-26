using System.Collections.Generic;

namespace NodeEditor
{
	public interface IHasValue
	{
		object value { get; }
	}

	public interface IHasValue<out T>
	{
		T value { get; }
	}
}