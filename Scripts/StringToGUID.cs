using System;
using System.Security.Cryptography;
using System.Text;

namespace NodeEditor
{
	public static class StringToGUID
	{
		public static Guid Get(string value)
		{
			using (MD5 md5Hasher = MD5.Create())
			{
				byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
				return new Guid(data);
			}
		}
	}
}