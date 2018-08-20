using System;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Editor.Scripts
{
	public class NodePort : Port
	{
		NodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
			: base(portOrientation, portDirection, portCapacity, type)
		{
			AddStyleSheetPath("Styles/NodePort");
		}

		NodeSlot m_Slot;

		public static Port Create(NodeSlot slot, IEdgeConnectorListener connectorListener)
		{
			var port = new NodePort(Orientation.Horizontal, slot.isInputSlot ? Direction.Input : Direction.Output,
				slot.isInputSlot ? Capacity.Single : Capacity.Multi, null)
			{
				m_EdgeConnector = new EdgeConnector<UnityEditor.Experimental.UIElements.GraphView.Edge>(connectorListener),
			};
			port.AddManipulator(port.m_EdgeConnector);
			port.slot = slot;
			port.portName = slot.displayName;
			port.portType = slot.valueType.Type;
			port.visualClass = slot.valueType.Type.Name;
			return port;
		}

		public NodeSlot slot
		{
			get { return m_Slot; }
			set
			{
				if (ReferenceEquals(value, m_Slot))
					return;
				if (value == null)
					throw new NullReferenceException();
				if (m_Slot != null && value.isInputSlot != m_Slot.isInputSlot)
					throw new ArgumentException("Cannot change direction of already created port");
				m_Slot = value;
				portName = slot.displayName;
				visualClass = slot.valueType.Type.Name;
			}
		}
	}

	static class NodePortExtensions
	{
		public static NodeSlot GetSlot(this Port port)
		{
			var shaderPort = port as NodePort;
			return shaderPort != null ? shaderPort.slot : null;
		}
	}
}