using System;
using NodeEditor.Nodes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine.UIElements;

namespace NodeEditor.Controls.Views
{
	public class ExposedReferenceControlView : VisualElement
	{
		public ExposedReferenceControlView(AbstractNode node, ReflectionProperty property)
		{
			var nodeType = node.GetType();
			if (nodeType.IsGenericType)
			{
				var genericNodeType = nodeType.GetGenericTypeDefinition();
				if (typeof(ExposedReferenceNode<>) == genericNodeType)
				{
					var referenceType = nodeType.GenericTypeArguments[0];
					Guid guid = (Guid) property.GetValue(node);
#if UNITY_EDITOR

					var objectField = new ObjectField();
					var propertyTable = node.owner.owner as IReferenceTable;
					var graphUnityObject = node.owner.owner as UnityEngine.Object;
                    objectField.objectType = referenceType;
					objectField.value = propertyTable != null ? propertyTable.GetReferenceValue(guid, out var isValid) : null;
					objectField.RegisterValueChangedCallback(e =>
					{
						if (propertyTable != null)
						{
							propertyTable.SetReferenceValue(guid, e.newValue);
						}
						else
						{
							objectField.value = null;
						}
						if(graphUnityObject != null) EditorUtility.SetDirty(graphUnityObject);
					});
					Add(objectField);
#endif
				}
			}
		}
	}
}