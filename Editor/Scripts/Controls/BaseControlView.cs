using System.Reflection;
using NodeEditor.Nodes;
using NodeEditor.Scripts.Views;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Editor.Controls
{
	[ControlElement(typeof(ControlAttribute))]
	public class BaseControlView : VisualElement
	{
		public BaseControlView(ControlAttribute attribute, AbstractNode node, PropertyInfo propertyInfo)
		{
			var propertyType = propertyInfo.PropertyType;
			if (propertyType == typeof(float)) AddControl( (ValueNode<float>)node, new FloatField(), propertyInfo);
			else if (node is ValueNode<bool>)
			{
				var toggle = new Toggle();
				toggle.value = (bool)propertyInfo.GetValue(node,null);
				toggle.OnToggle(() =>
				{
					node.owner.owner.RegisterCompleteObjectUndo("Boolean Change");
					propertyInfo.SetValue(node,toggle.value,null);
					node.Dirty(ModificationScope.Node);
				});
				Add(toggle);
			}
			else if (propertyType == typeof(double)) AddControl(node, new DoubleField(), propertyInfo);
			else if (propertyType == typeof(int)) AddControl((ValueNode<int>)node, new IntegerField(), propertyInfo);
			else if (propertyType == typeof(Color)) AddControl((ValueNode<Color>)node, new ColorField(), propertyInfo);
			else if (propertyType == typeof(Bounds)) AddControl((ValueNode<Bounds>)node, new BoundsField(), propertyInfo);
			else if (propertyType == typeof(Rect)) AddControl( (ValueNode<Rect>)node, new RectField(), propertyInfo);
			else if (propertyType == typeof(string)) AddControl( (ValueNode<string>)node, new TextField(), propertyInfo);
			else if (propertyType == typeof(Vector2)) Add(new MultiFloatSlotControlView(node, new[] { "x", "y" }, () => (Vector2)propertyInfo.GetValue(node,null), v => propertyInfo.SetValue(node,v,null)));
			else if (propertyType == typeof(Vector3)) Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z" }, () => (Vector3)propertyInfo.GetValue(node, null), v => propertyInfo.SetValue(node, v, null)));
			else if (propertyType == typeof(Vector4)) Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z", "w" }, () => (Vector4)propertyInfo.GetValue(node, null), v => propertyInfo.SetValue(node, v, null)));
			else if (propertyType == typeof(Quaternion)) Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z" }, () => ((Quaternion)propertyInfo.GetValue(node, null)).eulerAngles, v => propertyInfo.SetValue(node, Quaternion.Euler(v), null)));
			else if(typeof(Object).IsAssignableFrom(propertyType))
			{
				var objField = new ObjectField() { objectType = propertyType };
				objField.value = (Object)propertyInfo.GetValue(node,null);
				objField.OnValueChanged(e =>
				{
					node.owner.owner.RegisterCompleteObjectUndo(propertyType.Name + " Change");
					propertyInfo.SetValue(node,e.newValue,null);
					node.Dirty(ModificationScope.Node);
				});
				Add(objField);
			}
		}

		private void AddControl<T>(AbstractNode node, BaseTextControl<T> field,PropertyInfo property)
		{
			field.value = (T)property.GetValue(node,null);
			field.OnValueChanged(e =>
			{
				node.owner.owner.RegisterCompleteObjectUndo(typeof(T).Name + " Change");
				property.SetValue(node,e.newValue,null);
				node.Dirty(ModificationScope.Node);
			});
			Add(field);
		}

		private void AddControl<T>(AbstractNode node, BaseControl<T> field, PropertyInfo property)
		{
			field.value = (T)property.GetValue(node, null);
			field.OnValueChanged(e =>
			{
				node.owner.owner.RegisterCompleteObjectUndo(typeof(T).Name + " Change");
				property.SetValue(node, e.newValue, null);
				node.Dirty(ModificationScope.Node);
			});
			Add(field);
		}
	}
}