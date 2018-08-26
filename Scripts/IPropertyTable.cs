using System;

namespace NodeEditor
{
	public interface IPropertyTable
	{
		T GetPropertyValue<T>(Guid id, out bool validId);
		void ClearPropertyValue<T>(Guid id);
		void SetPropertyValue<T>(Guid id, T val);
	}
}