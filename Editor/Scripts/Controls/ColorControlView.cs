using System;
using System.Reflection;
using NodeEditor.Controls;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Editor.Controls
{
	[ControlElement(typeof(ColorControlAttribute))]
	public class ColorControlView : VisualElement
	{
		AbstractNode m_Node;
		PropertyInfo m_PropertyInfo;

		Color m_Color;
		ColorField m_ColorField;

		public ColorControlView(ColorControlAttribute attribute, AbstractNode node, PropertyInfo propertyInfo)
		{
			m_Node = node;
			m_PropertyInfo = propertyInfo;
			AddStyleSheetPath("Styles/Controls/ColorControlView");
			if (propertyInfo.PropertyType != typeof(Color))
				throw new ArgumentException("Property must be of type Color.", "propertyInfo");
			var label = attribute.label ?? ObjectNames.NicifyVariableName(propertyInfo.Name);

			m_Color = (Color)m_PropertyInfo.GetValue(m_Node, null);

			if (!string.IsNullOrEmpty(label))
				Add(new Label(label));

			m_ColorField = new ColorField { value = m_Color, showEyeDropper = false };
			m_ColorField.OnValueChanged(OnChange);
			Add(m_ColorField);
		}

		void OnChange(ChangeEvent<Color> evt)
		{
			m_Node.owner.owner.RegisterCompleteObjectUndo("Color Change");
			m_Color = evt.newValue;
			m_PropertyInfo.SetValue(m_Node, m_Color, null);
			Dirty(ChangeType.Repaint);
		}
	}
}