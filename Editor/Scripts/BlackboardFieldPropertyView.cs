using NodeEditor.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;
#if UNITY_2018_3_OR_NEWER
using ContextualMenu = UnityEngine.UIElements.DropdownMenu;
#endif

namespace NodeEditor.Scripts
{
    class BlackboardFieldPropertyView : VisualElement
    {
        readonly AbstractNodeGraph m_Graph;

        TextField m_Reference;

        IManipulator m_ResetReferenceMenu;

        public BlackboardFieldPropertyView(AbstractNodeGraph graph, INodeProperty property)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodeGraphBlackboard"));
            m_Graph = graph;

	        m_Reference = new TextField();
	        m_Reference.RegisterValueChangedCallback(evt =>
            {
                property.reference = evt.newValue;
                DirtyNodes(ModificationScope.Graph);
            });
	        m_Reference.value = property.reference;
            AddRow("Reference", m_Reference);

            if (property is ValueProperty<float>)
            {
                var floatProperty = (ValueProperty<float>)property;
	            FloatField floatField = new FloatField { value = floatProperty.value };
	            floatField.RegisterValueChangedCallback(evt =>
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
	            field.RegisterValueChangedCallback(intEvt =>
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
                field.RegisterValueChangedCallback(evt =>
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
                field.RegisterValueChangedCallback(evt =>
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
                field.RegisterValueChangedCallback(evt =>
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
                colorField.RegisterValueChangedCallback(evt =>
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
                field.RegisterValueChangedCallback(evt =>
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
                field.RegisterValueChangedCallback(evt =>
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
                field.RegisterValueChangedCallback(onBooleanChanged);
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
