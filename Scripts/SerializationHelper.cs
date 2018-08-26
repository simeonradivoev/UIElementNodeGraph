using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
	public static class SerializationHelper
	{
		[Serializable]
		public struct TypeSerializationInfo
		{
			[SerializeField]
			public string fullName;

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(fullName);
			}
		}

		[Serializable]
		public struct JSONSerializedElement
		{
			[SerializeField]
			public TypeSerializationInfo typeInfo;

			[SerializeField]
			public string JSONnodeData;
		}

		[Serializable]
		public struct JSONSerializedIndexedElement
		{
			[SerializeField]
			public TypeSerializationInfo typeInfo;

			[SerializeField]
			public int index;

			[SerializeField]
			public string JSONnodeData;
		}

		public static JSONSerializedElement nullElement => new JSONSerializedElement();

		public static TypeSerializationInfo GetTypeSerializableAsString(Type type)
		{
			return new TypeSerializationInfo
			{
				fullName = type.FullName
			};
		}

		static Type GetTypeFromSerializedString(TypeSerializationInfo typeInfo)
		{
			if (!typeInfo.IsValid())
				return null;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var type = assembly.GetType(typeInfo.fullName);
				if (type != null)
					return type;
			}

			return null;
		}

		public static JSONSerializedIndexedElement Serialize<T>(KeyValuePair<int, T> item, bool prettyPrint)
		{
			if (item.Value == null)
				throw new ArgumentNullException("item", "Can not serialize null element");

			var typeInfo = GetTypeSerializableAsString(item.Value.GetType());
			var data = JsonUtility.ToJson(item.Value, true);

			if (string.IsNullOrEmpty(data))
				throw new ArgumentException(string.Format("Can not serialize {0}", item.Value));
			;

			return new JSONSerializedIndexedElement
			{
				typeInfo = typeInfo,
				index = item.Key,
				JSONnodeData = data
			};
		}

		public static JSONSerializedElement Serialize<T>(T item,bool prettyPrint)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Can not serialize null element");

			var typeInfo = GetTypeSerializableAsString(item.GetType());
			var data = JsonUtility.ToJson(item, prettyPrint);

			if (string.IsNullOrEmpty(data))
				throw new ArgumentException(string.Format("Can not serialize {0}", item));
			;

			return new JSONSerializedElement
			{
				typeInfo = typeInfo,
				JSONnodeData = data
			};
		}

		static TypeSerializationInfo DoTypeRemap(TypeSerializationInfo info, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper)
		{
			TypeSerializationInfo foundInfo;
			if (remapper.TryGetValue(info, out foundInfo))
				return foundInfo;
			return info;
		}

		public static T Deserialize<T>(JSONSerializedIndexedElement item, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper, params object[] constructorArgs) where T : class
		{
			return Deserialize<T>(item.JSONnodeData, item.typeInfo, remapper, constructorArgs);
		}

		public static T Deserialize<T>(JSONSerializedElement item, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper, params object[] constructorArgs) where T : class
		{
			return Deserialize<T>(item.JSONnodeData, item.typeInfo, remapper, constructorArgs);
		}

		public static T Deserialize<T>(string data, TypeSerializationInfo typeInfo, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper,  params object[] constructorArgs) where T : class
		{
			if (!typeInfo.IsValid() || string.IsNullOrEmpty(data))
				throw new ArgumentException(string.Format("Can not deserialize {0} of type {1}, it is invalid", data, typeInfo));

			typeInfo.fullName = typeInfo.fullName.Replace("UnityEngine.MaterialGraph", "UnityEditor.ShaderGraph");
			typeInfo.fullName = typeInfo.fullName.Replace("UnityEngine.Graphing", "UnityEditor.Graphing");
			if (remapper != null)
				typeInfo = DoTypeRemap(typeInfo, remapper);

			var type = GetTypeFromSerializedString(typeInfo);
			if (type == null)
				throw new ArgumentException(string.Format("Can not deserialize ({0}), type is invalid", typeInfo.fullName));

			T instance;
			try
			{
				instance = Activator.CreateInstance(type, constructorArgs) as T;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Could not construct instance of: {0}", type), e);
			}

			if (instance != null)
			{
				JsonUtility.FromJsonOverwrite(data, instance);
				return instance;
			}
			return null;
		}

		public static List<JSONSerializedIndexedElement> Serialize<T>(IDictionary<int,T> dictionary, bool prettyPrint)
		{
			var result = new List<JSONSerializedIndexedElement>();
			if (dictionary == null)
				return result;

			foreach (var element in dictionary)
			{
				try
				{
					result.Add(Serialize(element, prettyPrint));
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			return result;
		}

		public static List<JSONSerializedElement> Serialize<T>(IEnumerable<T> list)
		{
			var result = new List<JSONSerializedElement>();
			if (list == null)
				return result;

			foreach (var element in list)
			{
				try
				{
					result.Add(Serialize(element,false));
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			return result;
		}

		public static List<T> Deserialize<T>(IEnumerable<JSONSerializedElement> list, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper, params object[] constructorArgs) where T : class
		{
			var result = new List<T>();
			if (list == null)
				return result;

			foreach (var element in list)
			{
				try
				{
					result.Add(Deserialize<T>(element, remapper));
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					Debug.LogError(element.JSONnodeData);
				}
			}
			return result;
		}

		public static void Deserialize<T>(IDictionary<int,T> dictionary,IEnumerable<JSONSerializedIndexedElement> list, Dictionary<TypeSerializationInfo, TypeSerializationInfo> remapper, params object[] constructorArgs) where T : class
		{
			foreach (var element in list)
			{
				try
				{
					T existingElement;
					if (dictionary.TryGetValue(element.index,out existingElement))
					{
						TypeSerializationInfo info = element.typeInfo;
						var type = GetTypeFromSerializedString(info);
						if (type == null)
							throw new ArgumentException(string.Format("Can not deserialize ({0}), type is invalid", info.fullName));

						if (!type.IsInstanceOfType(existingElement))
						{
							dictionary[element.index] = Deserialize<T>(element, remapper, constructorArgs);
						}
						else
						{
							JsonUtility.FromJsonOverwrite(element.JSONnodeData,existingElement);
						}
					}
					else
					{
						dictionary.Add(element.index,Deserialize<T>(element, remapper));
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					Debug.LogError(element.JSONnodeData);
				}
			}
		}
	}
}