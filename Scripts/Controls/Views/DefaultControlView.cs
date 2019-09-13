using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.Experimental.UIElements;
#endif

namespace NodeEditor.Controls.Views
{
	public class DefaultControlView : VisualElement
	{
		public const string ValueFieldName = "value-field";
		public const string ControlLabelName = "control-label";

		public DefaultControlView(DefaultControlAttribute attribute, AbstractNode node, ReflectionProperty property)
		{
			var viewCont = new VisualElement();
			viewCont.AddToClassList("ControlField");

			if (!string.IsNullOrEmpty(attribute.label))
				viewCont.Add(new Label(attribute.label){name = ControlLabelName });

			var propertyType = property.PropertyType;
			if(propertyType == typeof(bool))
			{
				var toggle = new Toggle() { name = ValueFieldName };
				toggle.value = (bool)property.GetValue(node);
				toggle.RegisterValueChangedCallback((e) =>
				{
					node.owner.owner.RegisterCompleteObjectUndo("Boolean Change");
					property.SetValue(node, e.newValue);
					node.Dirty(ModificationScope.Node);
				});
				viewCont.Add(toggle);
			}
#if UNITY_EDITOR
			else if (propertyType == typeof(float)) viewCont.Add(AddControl(node, new FloatField(){name = ValueFieldName }, property));
			else if (propertyType == typeof(double)) viewCont.Add(AddControl(node, new DoubleField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(int)) viewCont.Add(AddControl(node, new IntegerField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(Color)) viewCont.Add(AddControl(node, new ColorField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(Bounds)) viewCont.Add(AddControl(node, new BoundsField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(Rect)) viewCont.Add(AddControl(node, new RectField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(string)) viewCont.Add(AddControl(node, new TextField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(Gradient)) viewCont.Add(AddControl(node, new GradientField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(AnimationCurve)) viewCont.Add(AddControl(node, new CurveField() { name = ValueFieldName }, property));
			else if (propertyType == typeof(Vector2)) viewCont.Add(new MultiFloatSlotControlView(node, new[] { "x", "y" }, () => (Vector2)property.GetValue(node), v => property.SetValue(node,(Vector2)v)) { name = ValueFieldName });
			else if (propertyType == typeof(Vector3)) viewCont.Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z" }, () => (Vector3)property.GetValue(node), v => property.SetValue(node, (Vector3)v)) { name = ValueFieldName });
			else if (propertyType == typeof(Vector4)) viewCont.Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z", "w" }, () => (Vector4)property.GetValue(node), v => property.SetValue(node, v)) { name = ValueFieldName });
			else if (propertyType == typeof(Quaternion)) viewCont.Add(new MultiFloatSlotControlView(node, new[] { "x", "y", "z" }, () => ((Quaternion)property.GetValue(node)).eulerAngles, v => property.SetValue(node, Quaternion.Euler(v))) { name = ValueFieldName });
#endif

			if(viewCont.childCount > 0) Add(viewCont);
		}

		private TextInputBaseField<T> AddControl<T>(AbstractNode node, TextInputBaseField<T> field, ReflectionProperty property)
		{
			field.value = (T)property.GetValue(node);
			field.RegisterValueChangedCallback(e =>
			{
				node.owner.owner.RegisterCompleteObjectUndo(typeof(T).Name + " Change");
				property.SetValue(node,e.newValue);
				node.Dirty(ModificationScope.Node);
			});
			return field;
		}

		private BaseField<T> AddControl<T>(AbstractNode node, BaseField<T> field, ReflectionProperty property)
		{
			field.value = (T)property.GetValue(node);
			field.RegisterValueChangedCallback(e =>
			{
				node.owner.owner.RegisterCompleteObjectUndo(typeof(T).Name + " Change");
				property.SetValue(node, e.newValue);
				node.Dirty(ModificationScope.Node);
			});
			return field;
		}
	}
}