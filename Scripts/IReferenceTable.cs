using System;
using Object = UnityEngine.Object;

namespace NodeEditor
{
	/// <summary>
    /// An interface that denotes a reference table that provides references based on a GUID.
    /// </summary>
	public interface IReferenceTable
	{
		void SetReferenceValue(Guid id, Object value);
		Object GetReferenceValue(Guid id, out bool idValid);
		void ClearReferenceValue(Guid id);
	}
}