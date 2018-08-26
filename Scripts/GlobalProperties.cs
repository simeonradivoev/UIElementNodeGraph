using System;
using System.Collections.Generic;

namespace NodeEditor
{
	public static class GlobalProperties<T>
	{
		public static Dictionary<Guid, T> m_Values = new Dictionary<Guid, T>();

		public static bool TryGetValue(Guid key,out T val)
		{
			return m_Values.TryGetValue(key, out val);
		}

		public static void SetValue(Guid key, T val)
		{
			m_Values[key] = val;
		}

		public static void RemoveKey(Guid key)
		{
			m_Values.Remove(key);
		}
	}
}