using System;
using UnityEngine;

namespace NodeEditor.Scripts
{
	public static class TypeExtensions
	{
		public static Color32 GetColor(this Type type)
		{
			var hash = type.GetHashCode();
			return new Color32((byte)(hash & 0xFF), (byte)((hash >> 8) & 0xFF), (byte)((hash >> 16) & 0xFF), 255);
		}
	}
}