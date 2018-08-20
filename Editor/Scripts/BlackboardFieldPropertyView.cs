using NodeEditor.Nodes;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Toggle = UnityEngine.Experimental.UIElements.Toggle;
#if UNITY_2018_3_OR_NEWER
using ContextualMenu = UnityEngine.Experimental.UIElements.DropdownMenu;
#endif

namespace NodeEditor.Scripts
{
    class BlackboardFieldPropertyView : VisualElement
    {
        readonly AbstractNodeGraph m_Graph;

        Toggle m_ExposedToogle;

        IManipulator m_ResetReferenceMenu;

        public BlackboardFieldPropertyView(AbstractNodeGraph graph, INodeProperty property)
        {
            AddStyleSheetPath("Styles/NodeGraphBlackboard");
            m_Graph = graph;

            m_ExposedToogle = new Toggle();
            m_ExposedToogle.OnValueChanged(evt =>
            {
                property.exposed = evt.newValue;
                DirtyNodes(ModificationScope.Graph);
            });
            m_ExposedToogle.value = property.exposed;
            AddRow("Exposed", m_ExposedToogle);

            if (property is ValueProperty<float>)
            {
                var floatProperty = (ValueProperty<float>)property;
	            FloatField floatField = new FloatField { value = floatProperty.value };
	            floatField.OnValueChanged(evt =>
	            {
		            floatProperty.value = (float)evt.newValue;
		            DirtyNodes();
	            });
	            AddRow("Default", floatField);

                /*if (floatProperty.floatType == FloatType.Slider)
                {
                    var minField = new FloatField { value = floatProperty.rangeValues.x };
                    minField.OnValueChanged(minEvt =>
                        {
                            floatProperty.rangeValues = new Vector2((float)minEvt.newValue, floatProperty.rangeValues.y);
                            floatProperty.value = Mathf.Max(Mathf.Min(floatProperty.value, floatProperty.rangeValues.y), floatProperty.rangeValues.x);
                            floatField.value = floatProperty.value;
                            DirtyNodes();
                        });
                    minRow = AddRow("Min", minField);
                    var maxField = new FloatField { value = floatProperty.rangeValues.y };
                    maxField.OnValueChanged(maxEvt =>
                        {
                            floatProperty.rangeValues = new Vector2(floatProperty.rangeValues.x, (float)maxEvt.newValue);
                            floatProperty.value = Mathf.Max(Mathf.Min(floatProperty.value, floatProperty.rangeValues.y), floatProperty.rangeValues.x);
                            floatField.value = floatProperty.value;
                            DirtyNodes();
                        });
                    maxRow = AddRow("Max", maxField);
                }*/
            }else if (property is ValueProperty<int>)
            {
	            var intProperty = (ValueProperty<int>)property;

	            var field = new IntegerField { value = intProperty.value };
	            field.OnValueChanged(intEvt =>
	            {
		            intProperty.value = intEvt.newValue;
		            DirtyNodes();
	            });
	            AddRow("Default", field);
			}
            else if (property is ValueProperty<Vector2>)
            {
                var vectorProperty = (ValueProperty<Vector2>)property;
                var field = new Vector2Field { value = vectorProperty.value };
                field.OnValueChanged(evt =>
                    {
                        vectorProperty.value = evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", field);
            }
            else if (property is ValueProperty<Vector3>)
            {
                var vectorProperty = (ValueProperty<Vector3>)property;
                var field = new Vector3Field { value = vectorProperty.value };
                field.OnValueChanged(evt =>
                    {
                        vectorProperty.value = evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", field);
            }
            else if (property is ValueProperty<Vector4>)
            {
                var vectorProperty = (ValueProperty<Vector4>)property;
                var field = new Vector4Field { value = vectorProperty.value };
                field.OnValueChanged(evt =>
                    {
                        vectorProperty.value = evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", field);
            }
            else if (property is ValueProperty<Color>)
            {
                var colorProperty = (ValueProperty<Color>)property;
				//todo add HDR
                var colorField = new ColorField { value = (Color)property.defaultValue, showEyeDropper = false};
                colorField.OnValueChanged(evt =>
                    {
                        colorProperty.value = evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", colorField);
            }
            else if (property is ValueProperty<Texture2D>)
            {
                var textureProperty = (ValueProperty<Texture2D>)property;
                var field = new ObjectField { value = textureProperty.value, objectType = typeof(Texture2D) };
                field.OnValueChanged(evt =>
                    {
                        textureProperty.value = (Texture2D)evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", field);
            }
            else if (property is ValueProperty<Cubemap>)
            {
                var cubemapProperty = (ValueProperty<Cubemap>)property;
                var field = new ObjectField { value = cubemapProperty.value, objectType = typeof(Cubemap) };
                field.OnValueChanged(evt =>
                    {
                        cubemapProperty.value = (Cubemap)evt.newValue;
                        DirtyNodes();
                    });
                AddRow("Default", field);
            }
            else if (property is ValueProperty<bool>)
            {
                var booleanProperty = (ValueProperty<bool>)property;
                EventCallback<ChangeEvent<bool>> onBooleanChanged = evt =>
                    {
                        booleanProperty.value = evt.newValue;
                        DirtyNodes();
                    };
                var field = new Toggle();
                field.OnValueChanged(onBooleanChanged);
                field.value = booleanProperty.value;
                AddRow("Default", field);
            }
//            AddRow("Type", new TextField());
//            AddRow("Exposed", new Toggle(null));
//            AddRow("Range", new Toggle(null));
//            AddRow("Default", new TextField());
//            AddRow("Tooltip", new TextField());


            AddToClassList("sgblackboardFieldPropertyView");
        }

        VisualElement AddRow(string labelText, VisualElement control)
        {
            VisualElement rowView = new VisualElement();

            rowView.AddToClassList("rowView");

            Label label = new Label(labelText);

            label.AddToClassList("rowViewLabel");
            rowView.Add(label);

            control.AddToClassList("rowViewControl");
            rowView.Add(control);

            Add(rowView);
            return rowView;
        }

        void RemoveElements(VisualElement[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].parent == this)
                    Remove(elements[i]);
            }
        }

        void DirtyNodes(ModificationScope modificationScope = ModificationScope.Node)
        {
            foreach (var node in m_Graph.GetNodes<PropertyNode>())
                node.Dirty(modificationScope);
        }
    }
}
