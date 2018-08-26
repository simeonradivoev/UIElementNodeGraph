using System;
using Object = UnityEngine.Object;

namespace NodeEditor
{
	public interface IReferenceTable
	{
		void SetReferenceValue(Guid id, Object value);
		Object GetReferenceValue(Guid id, out bool idValid);
		void ClearReferenceValue(Guid id);
	}
}