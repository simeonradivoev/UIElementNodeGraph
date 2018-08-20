using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace NodeEditor.Editor.Scripts
{
    class EdgeConnectorListener : IEdgeConnectorListener
    {
        readonly AbstractNodeGraph m_Graph;
        readonly SearchWindowProvider m_SearchWindowProvider;

        public EdgeConnectorListener(AbstractNodeGraph graph, SearchWindowProvider searchWindowProvider)
        {
            m_Graph = graph;
            m_SearchWindowProvider = searchWindowProvider;
        }

        public void OnDropOutsidePort(UnityEditor.Experimental.UIElements.GraphView.Edge edge, Vector2 position)
        {
            var draggedPort = (edge.output != null ? edge.output.edgeConnector.edgeDragHelper.draggedPort : null) ?? (edge.input != null ? edge.input.edgeConnector.edgeDragHelper.draggedPort : null);
            m_SearchWindowProvider.connectedPort = (NodePort)draggedPort;
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), m_SearchWindowProvider);
        }

        public void OnDrop(GraphView graphView, UnityEditor.Experimental.UIElements.GraphView.Edge edge)
        {
            var leftSlot = edge.output.GetSlot();
            var rightSlot = edge.input.GetSlot();
            if (leftSlot != null && rightSlot != null)
            {
                m_Graph.owner.RegisterCompleteObjectUndo("Connect Edge");
                m_Graph.Connect(leftSlot.slotReference, rightSlot.slotReference);
            }
        }
    }
}
