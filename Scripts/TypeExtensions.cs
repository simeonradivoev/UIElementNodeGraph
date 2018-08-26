using System;

namespace NodeEditor.Scripts
{
	public static class TypeExtensions
	{
		public static bool IsInstanceOfGenericType(this Type type, object val)
		{
			var valType = val.GetType();
			if (!valType.IsGenericType) return false;
			return valType.GetGenericTypeDefinition() == type;
		}
	}
}
