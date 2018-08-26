using System;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
	[Serializable]
	public class SerializedType : ISerializationCallbackReceiver, IEquatable<SerializedType>
	{
		[NonSerialized] private Type type;
		[SerializeField] private string serialziedType;

		public Type Type => type;

		public SerializedType(Type type)
		{
			this.type = type;
		}

		public void OnBeforeSerialize()
		{
			serialziedType = type.FullName;
		}

		public void OnAfterDeserialize()
		{
			type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(serialziedType)).First(t => t != null);
		}

		public static implicit operator Type(SerializedType t)
		{
			return t.type;
		}

		public static implicit operator SerializedType(Type t)
		{
			return new SerializedType(t);
		}

		public override string ToString()
		{
			return type.ToString();
		}

		public static bool operator ==(SerializedType lhs, SerializedType rhs)
		{
			if (ReferenceEquals(lhs,null) || ReferenceEquals(rhs, null)) return false;
			return lhs.type == rhs.Type;
		}

		public static bool operator !=(SerializedType lhs, SerializedType rhs)
		{
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) return true;
			return lhs.type != rhs.Type;
		}

		public bool Equals(SerializedType other)
		{
			if (ReferenceEquals(other, null)) return false;
			return other.type == type;
		}

		public override bool Equals(object obj)
		{
			if (obj is SerializedType)
			{
				return Equals((SerializedType)obj);
			}
			if (obj is Type)
			{
				return (Type)obj == type;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return type.GetHashCode();
		}
	}
}