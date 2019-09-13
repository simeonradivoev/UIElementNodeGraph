using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;

namespace NodeEditor.Scripts.Views
{
    public class PortInputView : GraphElement, IDisposable
    {
        readonly CustomStyleProperty<Color> k_EdgeColorProperty = new CustomStyleProperty<Color>("--edge-color");

        Color m_EdgeColor = Color.red;

        public Color edgeColor => m_EdgeColor;

        public NodeSlot slot => m_Slot;

        NodeSlot m_Slot;
        SerializedType m_SlotType;
        VisualElement m_Control;
        VisualElement m_Container;
        EdgeControl m_EdgeControl;

        public PortInputView(NodeSlot slot)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PortInputView"));
            pickingMode = PickingMode.Ignore;
            ClearClassList();
            m_Slot = slot;
            m_SlotType = slot.valueType;
            AddToClassList("type" + m_SlotType.Type.Name);

            m_EdgeControl = new EdgeControl
            {
                @from = new Vector2(212f - 21f, 11.5f),
                to = new Vector2(212f, 11.5f),
                edgeWidth = 2,
                pickingMode = PickingMode.Ignore
            };
            Add(m_EdgeControl);

            m_Container = new VisualElement { name = "container" };
            {
                m_Control = this.slot.InstantiateControl();
                if (m_Control != null)
                    m_Container.Add(m_Control);

				var slotElement = new VisualElement { name = "slot" };
                {
                    slotElement.Add(new VisualElement { name = "dot" });
                }
                m_Container.Add(slotElement);
            }
            Add(m_Container);

            m_Container.visible = m_EdgeControl.visible = m_Control != null;

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        protected void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            Color colorValue;

            if (e.customStyle.TryGetValue(k_EdgeColorProperty, out colorValue))
                m_EdgeColor = colorValue;

            m_EdgeControl.UpdateLayout();
            m_EdgeControl.inputColor = edgeColor;
            m_EdgeControl.outputColor = edgeColor;
        }

        public void UpdateSlot(NodeSlot newSlot)
        {
            m_Slot = newSlot;
            Recreate();
        }

        public void UpdateSlotType()
        {
            if (slot.valueType != m_SlotType)
                Recreate();
        }

        void Recreate()
        {
            RemoveFromClassList("type" + m_SlotType.Type.Name);
            m_SlotType = slot.valueType;
            AddToClassList("type" + m_SlotType.Type.Name);
            if (m_Control != null)
            {
                var disposable = m_Control as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                m_Container.Remove(m_Control);
            }
            m_Control = slot.InstantiateControl();
            if (m_Control != null)
                m_Container.Insert(0, m_Control);

            m_Container.visible = m_EdgeControl.visible = m_Control != null;
        }

        public void Dispose()
        {
            var disposable = m_Control as IDisposable;
            disposable?.Dispose();
        }
    }
}
