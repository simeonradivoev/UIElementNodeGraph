using System;

namespace NodeEditor
{
	public class GuidEncoder
	{
		public static string Encode(Guid guid)
		{
			string enc = Convert.ToBase64String(guid.ToByteArray());
			return $"{enc.GetHashCode():X}";
		}
	}
}