using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor
{
	public class PropertyRow : VisualElement
	{
		VisualElement m_ContentContainer;
		VisualElement m_LabelContainer;

		public override VisualElement contentContainer => m_ContentContainer;

		public VisualElement label
		{
			get => m_LabelContainer.Children().FirstOrDefault();
            set
			{
				var first = m_LabelContainer.Children().FirstOrDefault();
                first?.RemoveFromHierarchy();
                m_LabelContainer.Add(value);
			}
		}

		public PropertyRow(VisualElement label = null)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertyRow"));
			VisualElement container = new VisualElement {name = "container"};
			m_ContentContainer = new VisualElement { name = "content"  };
			m_LabelContainer = new VisualElement {name = "label" };
			m_LabelContainer.Add(label);

			container.Add(m_LabelContainer);
			container.Add(m_ContentContainer);

			hierarchy.Add(container);
		}
	}
}