using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor
{
	public class PropertySheet : VisualElement
	{
		VisualElement m_ContentContainer;
		VisualElement m_HeaderContainer;
		Label m_Header;
		public override VisualElement contentContainer => m_ContentContainer;

		public VisualElement headerContainer
		{
			get => m_HeaderContainer.Children().FirstOrDefault();
            set
			{
				var first = m_HeaderContainer.Children().FirstOrDefault();
                first?.RemoveFromHierarchy();

                m_HeaderContainer.Add(value);
			}
		}

		public PropertySheet(Label header = null)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertySheet"));
			m_ContentContainer = new VisualElement { name = "content" };
			m_HeaderContainer = new VisualElement { name = "header" };
			if (header != null)
				m_HeaderContainer.Add(header);

			m_ContentContainer.Add(m_HeaderContainer);
            hierarchy.Add(m_ContentContainer);
		}
	}
}