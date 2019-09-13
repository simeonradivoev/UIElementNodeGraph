using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor.Scripts
{
	public class NodePort : Port
	{
		NodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
			: base(portOrientation, portDirection, portCapacity, type)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodePort"));
		}

		NodeSlot m_Slot;

		public static Port Create(NodeSlot slot, IEdgeConnectorListener connectorListener)
		{
			var port = new NodePort(Orientation.Horizontal, slot.isInputSlot ? Direction.Input : Direction.Output,
				slot.isInputSlot || slot.allowMultipleConnections ? Capacity.Single : Capacity.Multi, null)
			{
				m_EdgeConnector = new EdgeConnector<UnityEditor.Experimental.GraphView.Edge>(connectorListener),
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
			get => m_Slot;
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
            return port is NodePort shaderPort ? shaderPort.slot : null;
		}
	}
}