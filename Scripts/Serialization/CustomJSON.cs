using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NodeEditor;
using NodeEditor.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleJSON
{
	public partial class JSONNode
    {
		public static implicit operator JSONNode(Guid n)
		{
			return new JSONString(n.ToString());
		}

		public static implicit operator Guid(JSONNode d)
		{
			return (d == null) ? new Guid() : Guid.Parse(d.Value);
		}

		public static implicit operator JSONNode(Vector2 n)
		{
			return new JSONArray(n.x, n.y);
		}

		public static implicit operator Vector2(JSONNode d)
		{
			return (d == null) ? new Vector2() : new Vector2(d[0], d[1]);
		}

		public static implicit operator JSONNode(Vector3 n)
		{
			return new JSONArray(n.x, n.y,n.z);
		}

		public static implicit operator Vector3(JSONNode d)
		{
			return (d == null) ? new Vector3() : new Vector3(d[0], d[1], d[2]);
		}

        public static implicit operator JSONNode(Rect n)
		{
			return new JSONArray(n.x, n.y, n.width, n.height);
		}

		public static implicit operator Rect(JSONNode d)
		{
			return (d == null) ? new Rect() : new Rect(d[0], d[1], d[2], d[3]);
		}

		public static implicit operator JSONNode(Type n)
		{
			return new JSONString(n.FullName);
		}

		public static implicit operator Type(JSONNode d)
		{
			if (d == null)
				return null;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var type = assembly.GetType(d.Value);
				if (type != null)
					return type;
			}

			return null;
		}
    }

	public static class JSONExtensions
	{
		private abstract class SerializationHandler
		{
			public abstract bool IsValid(Type type);
			public abstract object Deserialize(JSONNode node,Type fieldType);
			public abstract JSONNode Serialize(object value);
		}

		private class SerializationHandlerFunction : SerializationHandler
		{
			private Func<Type, bool> m_IsValid;
			private Func<JSONNode,Type, object> m_Deserialize;
			private Func<object, JSONNode> m_Serialize;

			public override bool IsValid(Type type) => m_IsValid?.Invoke(type) ?? false;
			public override object Deserialize(JSONNode node, Type rootType) => m_Deserialize.Invoke(node,rootType);
			public override JSONNode Serialize(object value) => m_Serialize.Invoke(value);

			public SerializationHandlerFunction(Func<Type, bool> isValid, Func<JSONNode, Type, object> deserialize, Func<object, JSONNode> serialize)
			{
				m_IsValid = isValid;
				m_Deserialize = deserialize;
				m_Serialize = serialize;
			}

			public SerializationHandlerFunction(Func<JSONNode, Type, object> deserialize, Func<object, JSONNode> serialize)
			{
				m_Deserialize = deserialize;
				m_Serialize = serialize;
			}
        }

		private class DictionarySerializationHandler : SerializationHandler
		{
			public override bool IsValid(Type type)
			{
				return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
			}

			public override object Deserialize(JSONNode node, Type rootType)
			{
				var dictionaryInstance = (IDictionary)Activator.CreateInstance(rootType);
				Type keyType = rootType.GetGenericArguments()[0];
				Type valueType = rootType.GetGenericArguments()[1];

				node["$type"] = rootType;
				var kvpData = node["$kvp"].AsArray;

                foreach (var kvp in kvpData.Children)
				{
					var keyData = kvp["$key"];
					var valueData = kvp["$value"];
					dictionaryInstance.Add(keyData.FromReflection((Type)keyData["$type"] ?? keyType),valueData.FromReflection((Type)valueData["$type"] ?? valueType));
				}

				return dictionaryInstance;
			}

			public override JSONNode Serialize(object value)
			{
				Type dictionaryType = value.GetType();

				var keyValues = new JSONArray();

				var dictionary = (IDictionary)value;

				foreach (var key in dictionary.Keys)
				{
					var kvpData = new JSONObject
					{
						["$key"] = SerializeFromReflection(key),
						["$value"] = SerializeFromReflection(dictionary[key])
					};
					keyValues.Add(kvpData);
                }

				return new JSONObject()
				{
					["$type"] = dictionaryType,
					["$kvp"] = keyValues
				};
			}
		}

		private class GenericSerializationHandler : SerializationHandler
		{
			public override bool IsValid(Type type)
			{
				return true;
			}

			public override object Deserialize(JSONNode node, Type rootType)
			{
				var instance = FormatterServices.GetSafeUninitializedObject(rootType);

				var fields = ListPool<FieldInfo>.Get();
				fields.AddRange(rootType.GetFields(BindingFlags.Public | BindingFlags.Instance));
				GetAllPrivateFields(rootType, fields);

				foreach (var fieldInfo in fields)
                {
                    var fieldData = node[fieldInfo.Name];
                    if (fieldData == null)
                    {
                        var oldNameAttributes = fieldInfo.GetCustomAttributes<FormerlySerializedAsAttribute>();
                        foreach (var attribute in oldNameAttributes)
                        {
                            fieldData = node[attribute.oldName];
                            if (fieldData != null)
                            {
                                break;
                            }
                        }
                    }

                    if (fieldData != null)
                    {
	                    fieldInfo.SetValue(instance, FromReflection(fieldData, (Type)fieldData["$type"] ?? fieldInfo.FieldType));
                    }
                }

				ListPool<FieldInfo>.Release(fields);

                return instance;
			}

			public override JSONNode Serialize(object value)
			{
				Type type = value.GetType();
				JSONNode root = new JSONObject();
				root["$type"] = type;

				var fields = ListPool<FieldInfo>.Get();
				fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
				GetAllPrivateFields(type,fields);

				foreach (var publicField in fields)
				{
					var fieldValue = publicField.GetValue(value);
					var fieldData = SerializeFromReflection(fieldValue);
					if (fieldData != null)
					{
						root[publicField.Name] = fieldData;
                    }
				}

				ListPool <FieldInfo>.Release(fields);

				return root;
            }
		}

		private class ListSerializationHandler : SerializationHandler
		{
			public override bool IsValid(Type type)
			{
				return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            }

			public override object Deserialize(JSONNode node, Type rootType)
			{
				var listInstance = (IList)Activator.CreateInstance(rootType);
				Type elementType = rootType.GetGenericArguments()[0];

				var listData = node["$list"].AsArray;

				foreach (var kvp in listData.Children)
				{
					listInstance.Add(kvp.FromReflection((Type)kvp["$type"] ?? elementType));
				}

				return listInstance;
            }

			public override JSONNode Serialize(object value)
			{
				Type listType = value.GetType();

				var listData = new JSONArray();

				var list = (IList)value;

				foreach (var element in list)
				{
					listData.Add(SerializeFromReflection(element));
				}

				return new JSONObject()
				{
					["$type"] = listType,
					["$list"] = listData
                };
            }
		}

		private class ArraySerializationHandler : SerializationHandler
		{
			public override bool IsValid(Type type)
			{
				return type.IsArray;
			}

			public override object Deserialize(JSONNode node, Type rootType)
			{
				Type elementType = rootType.GetElementType();

				var arrayData = node["$array"].AsArray;

				var arrayInstance = Array.CreateInstance(rootType.GetElementType(), arrayData.Count);

				for (int i = 0; i < arrayData.Count; i++)
				{
					var elementData = arrayData[i];
					arrayInstance.SetValue(elementData.FromReflection((Type)elementData["$type"] ?? elementType),i);
				}

				return arrayInstance;
			}

			public override JSONNode Serialize(object value)
			{
				Type arrayType = value.GetType();
				Type elementType = arrayType.GetElementType();

				var arrayData = new JSONArray();

				var array = (Array)value;

				foreach (var key in array)
				{
					arrayData.Add(SerializeFromReflection(key));
				}

				return new JSONObject()
				{
					["$type"] = arrayType,
					["$array"] = arrayData
				};
			}
		}

        private static void GetAllPrivateFields(Type t, List<FieldInfo> infos)
		{
			if (t == null)
				return;

			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			foreach (var fieldInfo in t.GetFields(flags))
			{
				if (fieldInfo.GetCustomAttribute<SerializeField>() != null)
				{
					infos.Add(fieldInfo);
                }
			}

			GetAllPrivateFields(t.BaseType,infos);
		}

        private static List<SerializationHandler> s_Handlers;
        private static Dictionary<Type,SerializationHandler> s_ConcreteHandlers;

        static JSONExtensions()
        {
	        s_ConcreteHandlers = new Dictionary<Type, SerializationHandler>()
	        {
		        [typeof(float)] = new SerializationHandlerFunction((e,t) => (float) e, e => (float) e),
		        [typeof(double)] = new SerializationHandlerFunction((e, t) => (double) e, e => (double) e),
		        [typeof(string)] = new SerializationHandlerFunction((e, t) => (string) e, e => (string) e),
		        [typeof(Guid)] = new SerializationHandlerFunction((e, t) => (Guid) e, e => (Guid) e),
		        [typeof(Rect)] = new SerializationHandlerFunction((e, t) => (Rect) e, e => (Rect) e),
		        [typeof(Vector2)] = new SerializationHandlerFunction((e, t) => (Vector2) e, e => (Vector2) e),
		        [typeof(Vector3)] = new SerializationHandlerFunction((e, t) => (Vector3) e, e => (Vector3) e),
		        [typeof(bool)] = new SerializationHandlerFunction((e, t) => (bool) e, e => (bool) e),
		        [typeof(long)] = new SerializationHandlerFunction((e, t) => (long) e, e => (long) e),
		        [typeof(int)] = new SerializationHandlerFunction((e, t) => (int) e, e => (int) e),
		        [typeof(char)] = new SerializationHandlerFunction((e, t) => (char) e, e => (char) e),
		        [typeof(short)] = new SerializationHandlerFunction((e, t) => (short) e, e => (short) e),
		        [typeof(byte)] = new SerializationHandlerFunction((e, t) => (byte) e, e => (byte) e),
	        };

	        s_Handlers = new List<SerializationHandler>
	        {
		        new GenericSerializationHandler(),
		        new ArraySerializationHandler(),
                new DictionarySerializationHandler(),
				new ListSerializationHandler()
	        };
        }

        public static JSONNode SerializeFromReflection(object n)
        {
	        JSONNode val;

	        if (n == null)
	        {
		        val = JSONNull.CreateOrGet();
	        }
	        else
	        {
		        Type rootType = n.GetType();

                if (n is ISerializationCallbackReceiver newValueCallback)
		        {
			        newValueCallback.OnBeforeSerialize();
		        }

		        if (s_ConcreteHandlers.TryGetValue(rootType, out var converter))
		        {
			        val = converter.Serialize(n);
		        }
		        else
		        {
			        var handler = s_Handlers.LastOrDefault(h => h.IsValid(rootType));
			        val = handler?.Serialize(n);
		        }
            }

	        return val;
        }

        public static object FromReflection(this JSONNode root, Type rootType)
        {
	        object val;

	        if (root.IsNull)
	        {
		        val = rootType.IsValueType ? Activator.CreateInstance(rootType) : null;
            }
	        else
	        {
		        if (s_ConcreteHandlers.TryGetValue(rootType, out var converter))
		        {
			        val = converter.Deserialize(root, rootType);
		        }
		        else
		        {
			        var handler = s_Handlers.LastOrDefault(h => h.IsValid(rootType));
			        if (handler != null)
			        {
				        val = handler.Deserialize(root, rootType);
			        }
			        else
			        {
				        val = rootType.IsValueType ? Activator.CreateInstance(rootType) : null;
			        }
		        }

		        if (val is ISerializationCallbackReceiver newValueCallback)
		        {
			        newValueCallback.OnAfterDeserialize();
		        }
            }

	        return val;
        }
	}
}