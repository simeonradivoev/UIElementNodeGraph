using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NodeEditor.Util;

namespace NodeEditor
{
	public static class GraphUtil
	{
		internal static string ConvertCamelCase(string text, bool preserveAcronyms)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;
			StringBuilder newText = new StringBuilder(text.Length * 2);
			newText.Append(text[0]);
			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]))
					if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
					    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
					     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
						newText.Append(' ');
				newText.Append(text[i]);
			}
			return newText.ToString();
		}

		static Dictionary<SerializationHelper.TypeSerializationInfo, SerializationHelper.TypeSerializationInfo> s_LegacyTypeRemapping;

		public static Dictionary<SerializationHelper.TypeSerializationInfo, SerializationHelper.TypeSerializationInfo> GetLegacyTypeRemapping()
		{
			if (s_LegacyTypeRemapping == null)
			{
				s_LegacyTypeRemapping = new Dictionary<SerializationHelper.TypeSerializationInfo, SerializationHelper.TypeSerializationInfo>();
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypesOrNothing())
					{
						if (type.IsAbstract)
							continue;
						foreach (var attribute in type.GetCustomAttributes(typeof(FormerNameAttribute), false))
						{
							var legacyAttribute = (FormerNameAttribute)attribute;
							var serializationInfo = new SerializationHelper.TypeSerializationInfo { fullName = legacyAttribute.fullName };
							s_LegacyTypeRemapping[serializationInfo] = SerializationHelper.GetTypeSerializableAsString(type);
						}
					}
				}
			}

			return s_LegacyTypeRemapping;
		}

		/// <summary>
		/// Sanitizes a supplied string such that it does not collide
		/// with any other name in a collection.
		/// </summary>
		/// <param name="existingNames">
		/// A collection of names that the new name should not collide with.
		/// </param>
		/// <param name="duplicateFormat">
		/// The format applied to the name if a duplicate exists.
		/// This must be a format string that contains `{0}` and `{1}`
		/// once each. An example could be `{0} ({1})`, which will append ` (n)`
		/// to the name for the n`th duplicate.
		/// </param>
		/// <param name="name">
		/// The name to be sanitized.
		/// </param>
		/// <returns>
		/// A name that is distinct form any name in `existingNames`.
		/// </returns>
		internal static string SanitizeName(IEnumerable<string> existingNames, string duplicateFormat, string name)
		{
			if (!existingNames.Contains(name))
				return name;

			string escapedDuplicateFormat = Regex.Escape(duplicateFormat);

			// Escaped format will escape string interpolation, so the escape caracters must be removed for these.
			escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{0}", @"{0}");
			escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{1}", @"{1}");

			var baseRegex = new Regex(string.Format(escapedDuplicateFormat, @"^(.*)", @"(\d+)"));

			var baseMatch = baseRegex.Match(name);
			if (baseMatch.Success)
				name = baseMatch.Groups[1].Value;

			string baseNameExpression = string.Format(@"^{0}", Regex.Escape(name));
			var regex = new Regex(string.Format(escapedDuplicateFormat, baseNameExpression, @"(\d+)") + "$");

			var existingDuplicateNumbers = existingNames.Select(existingName => regex.Match(existingName)).Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).Where(n => n > 0).Distinct().ToList();

			var duplicateNumber = 1;
			existingDuplicateNumbers.Sort();
			if (existingDuplicateNumbers.Any() && existingDuplicateNumbers.First() == 1)
			{
				duplicateNumber = existingDuplicateNumbers.Last() + 1;
				for (var i = 1; i < existingDuplicateNumbers.Count; i++)
				{
					if (existingDuplicateNumbers[i - 1] != existingDuplicateNumbers[i] - 1)
					{
						duplicateNumber = existingDuplicateNumbers[i - 1] + 1;
						break;
					}
				}
			}

			return string.Format(duplicateFormat, name, duplicateNumber);
		}
	}
}