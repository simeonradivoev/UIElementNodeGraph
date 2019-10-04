using System.Collections.Generic;

namespace NodeEditor
{
	/// <summary>
    /// Interface for Nodes that contain a value.
    /// </summary>
	public interface IHasValue
	{
		object value { get; }
	}

	/// <summary>
	/// Interface for Nodes that contain a value.
	/// </summary>
    public interface IHasValue<out T>
	{
		T value { get; }
	}
}