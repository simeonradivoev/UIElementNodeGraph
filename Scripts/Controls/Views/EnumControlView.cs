using System;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif
using UnityEngine.UIElements;

namespace NodeEditor.Controls.Views
{
	public class EnumControlView : VisualElement
	{
		public EnumControlView(ControlAttribute attribute, AbstractNode node, ReflectionProperty property)
		{
			var viewCont = new VisualElement();
			viewCont.AddToClassList("ControlField");

			if (!string.IsNullOrEmpty(attribute.label))
				viewCont.Add(new Label(attribute.label){name = DefaultControlView.ControlLabelName });

#if UNITY_EDITOR
			var enumField = new EnumField((Enum)property.GetValue(node)){name = DefaultControlView.ValueFieldName};
			enumField.RegisterValueChangedCallback(e =>
			{
				node.owner.owner.RegisterCompleteObjectUndo("Enum Change");
				property.SetValue(node, e.newValue);
				node.Dirty(ModificationScope.Node);
			});
			viewCont.Add(enumField);
#endif
			Add(viewCont);
		}
	}
}