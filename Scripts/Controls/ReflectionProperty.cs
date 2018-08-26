using System;
using System.Reflection;

namespace NodeEditor.Controls
{
	public class ReflectionProperty
	{
		private PropertyInfo m_PropertyInfo;
		private FieldInfo m_FieldInfo;

		public ReflectionProperty(PropertyInfo mPropertyInfo)
		{
			m_PropertyInfo = mPropertyInfo;
		}

		public ReflectionProperty(FieldInfo mFieldInfo)
		{
			m_FieldInfo = mFieldInfo;
		}

		public void SetValue(object obj, object value)
		{
			if(m_PropertyInfo != null) m_PropertyInfo.SetValue(obj,value,null);
			else m_FieldInfo.SetValue(obj,value);
		}

		public object GetValue(object obj)
		{
			if (m_PropertyInfo != null) return m_PropertyInfo.GetValue(obj, null);
			return m_FieldInfo.GetValue(obj);
		}

		public Type PropertyType
		{
			get
			{
				if (m_PropertyInfo != null) return m_PropertyInfo.PropertyType;
				return m_FieldInfo.FieldType;
			}
		}
	}
}