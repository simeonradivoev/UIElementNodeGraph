using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeEditor.Controls;
using NodeEditor.Util;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace NodeEditor.Scripts.Views
{
	public class NodeView : Node
	{
		TextField m_TitleField;
		VisualElement m_TitleLabel;
		VisualElement m_PriorityField;
		VisualElement m_ControlItems;
		VisualElement m_ControlsDivider;
		IEdgeConnectorListener m_ConnectorListener;
		VisualElement m_PortInputContainer;
		VisualElement m_SettingsContainer;
		bool m_ShowSettings = false;
		VisualElement m_SettingsButton;
		VisualElement m_Settings;
		VisualElement m_NodeSettingsView;
		private bool m_EditPathCancelled;


		public void Initialize(AbstractNode inNode, IEdgeConnectorListener connectorListener)
		{
			AddStyleSheetPath("Styles/NodeView");
			AddToClassList("Node");

			if (inNode == null)
				return;

			var contents = this.Q("contents");

			m_ConnectorListener = connectorListener;
			node = inNode;
			persistenceKey = node.guid.ToString();
			var priorityField = new IntegerField() {value = node.priority,name = "priority-field",visible = CalculatePriorityVisibility(node)};
			priorityField.OnValueChanged(e => node.priority = e.newValue);
			titleContainer.Insert(1,priorityField);
			m_PriorityField = priorityField;
			m_TitleLabel = titleContainer.First(e => e.name == "title-label");
			m_TitleLabel.RegisterCallback<MouseDownEvent>(OnTitleLabelMouseDown);
			m_TitleField = new TextField
			{
				name = "title-field",
				value = inNode.name,
				visible = false
			};
			m_TitleField.style.positionType = PositionType.Absolute;
			m_TitleField.RegisterCallback<FocusOutEvent>(e => { OnEditTitleTextFinished(); });
			m_TitleField.RegisterCallback<KeyDownEvent>(OnTitleTextFieldKeyPressed);
			titleContainer.shadow.Add(m_TitleField);
			UpdateTitle();

			// Add controls container
			var controlsContainer = new VisualElement { name = "controls" };
			{
				m_ControlsDivider = new VisualElement { name = "divider" };
				m_ControlsDivider.AddToClassList("horizontal");
				controlsContainer.Add(m_ControlsDivider);
				m_ControlItems = new VisualElement { name = "items" };
				controlsContainer.Add(m_ControlItems);

				// Instantiate control views from node
				foreach (var propertyInfo in node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				foreach (IControlAttribute attribute in propertyInfo.GetCustomAttributes(typeof(IControlAttribute), false))
				{
					var control = attribute.InstantiateControl(node, new ReflectionProperty(propertyInfo));
					if(control != null) m_ControlItems.Add(control);
				}

				foreach (var fieldInfo in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				foreach (IControlAttribute attribute in fieldInfo.GetCustomAttributes(typeof(IControlAttribute), false))
				{
					var control = attribute.InstantiateControl(node, new ReflectionProperty(fieldInfo));
					if (control != null) m_ControlItems.Add(control);
				}
			}
			if (m_ControlItems.childCount > 0)
				contents.Add(controlsContainer);

			// Add port input container, which acts as a pixel cache for all port inputs
			m_PortInputContainer = new VisualElement
			{
				name = "portInputContainer",
				clippingOptions = ClippingOptions.ClipAndCacheContents,
				pickingMode = PickingMode.Ignore
			};
			Add(m_PortInputContainer);

			AddSlots(node.GetSlots<NodeSlot>());
			UpdatePortInputs();
			base.expanded = node.drawState.expanded;
			RefreshExpandedState(); //This should not be needed. GraphView needs to improve the extension api here
			UpdatePortInputVisibilities();

			SetPosition(new Rect(node.drawState.position.x, node.drawState.position.y, 0, 0));

			/*if (node is SubGraphNode)
			{
				RegisterCallback<MouseDownEvent>(OnSubGraphDoubleClick);
			}*/

			m_PortInputContainer.SendToBack();

			// Remove this after updated to the correct API call has landed in trunk. ------------
			VisualElement m_TitleContainer;
			VisualElement m_ButtonContainer;
			m_TitleContainer = this.Q("title");
			// -----------------------------------------------------------------------------------

			var settings = node as IHasSettings;
			if (settings != null)
			{
				m_NodeSettingsView = new NodeSettingsView();
				m_NodeSettingsView.visible = false;

				Add(m_NodeSettingsView);

				m_SettingsButton = new VisualElement { name = "settings-button" };
				m_SettingsButton.Add(new VisualElement { name = "icon" });

				m_Settings = settings.CreateSettingsElement();

				m_SettingsButton.AddManipulator(new Clickable(() =>
				{
					UpdateSettingsExpandedState();
				}));

				// Remove this after updated to the correct API call has landed in trunk. ------------
				m_ButtonContainer = new VisualElement { name = "button-container" };
				m_ButtonContainer.style.flexDirection = StyleValue<FlexDirection>.Create(FlexDirection.Row);
				m_ButtonContainer.Add(m_SettingsButton);
				m_ButtonContainer.Add(m_CollapseButton);
				m_TitleContainer.Add(m_ButtonContainer);
				// -----------------------------------------------------------------------------------
				//titleButtonContainer.Add(m_SettingsButton);
				//titleButtonContainer.Add(m_CollapseButton);

				RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			}
		}

		private void OnTitleLabelMouseDown(MouseDownEvent evt)
		{
			if (evt.clickCount == 2 && evt.button == (int)MouseButton.LeftMouse)
			{
				StartEditingName();
				evt.PreventDefault();
			}
		}

		private void StartEditingName()
		{
			m_TitleField.visible = true;

			m_TitleField.value = node.name;
			m_TitleField.style.positionType = PositionType.Absolute;
			m_TitleField.style.positionLeft = titleContainer.layout.x;
			m_TitleField.style.positionTop = titleContainer.layout.y;
			m_TitleField.style.width = titleContainer.layout.width;
			m_TitleField.style.height = titleContainer.layout.height;
			m_TitleField.style.fontSize = m_TitleLabel.style.fontSize;
			m_TitleField.style.marginLeft = 0;
			m_TitleField.style.marginRight = 0;
			m_TitleField.style.marginTop = 0;
			m_TitleField.style.marginBottom = 0;
			m_TitleField.style.textAlignment = TextAnchor.MiddleLeft;

			m_TitleLabel.visible = false;

			m_TitleField.Focus();
			m_TitleField.SelectAll();
		}

		void OnTitleTextFieldKeyPressed(KeyDownEvent evt)
		{
			switch (evt.keyCode)
			{
				case KeyCode.Escape:
					m_EditPathCancelled = true;
					m_TitleField.Blur();
					break;
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					m_TitleField.Blur();
					break;
				default:
					break;
			}
		}

		void OnEditTitleTextFinished()
		{
			m_TitleLabel.visible = true;
			m_TitleField.visible = false;

			var newPath = m_TitleField.text;
			/*if (!m_EditPathCancelled && (newPath != node.name.text))
			{
				newPath = SanitizePath(newPath);
			}*/

			node.name = newPath;
			node.Dirty(ModificationScope.Node);
			UpdateTitle();
			m_EditPathCancelled = false;
		}

		void OnGeometryChanged(GeometryChangedEvent evt)
		{
			// style.positionTop and style.positionLeft are in relation to the parent,
			// so we translate the layout of the settings button to be in the coordinate
			// space of the settings view's parent.

			var settingsButtonLayout = m_SettingsButton.ChangeCoordinatesTo(m_NodeSettingsView.parent, m_SettingsButton.layout);
			m_NodeSettingsView.style.positionTop = settingsButtonLayout.yMax - 18f;
			m_NodeSettingsView.style.positionLeft = settingsButtonLayout.xMin - 16f;
		}

		/*void OnSubGraphDoubleClick(MouseDownEvent evt)
		{
			if (evt.clickCount == 2 && evt.button == 0)
			{
				SubGraphNode subgraphNode = node as SubGraphNode;

				var path = AssetDatabase.GetAssetPath(subgraphNode.subGraphAsset);
				ShaderGraphImporterEditor.ShowGraphEditWindow(path);
			}
		}*/

		public AbstractNode node { get; private set; }

		public override bool expanded
		{
			get { return base.expanded; }
			set
			{
				if (base.expanded != value)
					base.expanded = value;

				if (node.drawState.expanded != value)
				{
					var ds = node.drawState;
					ds.expanded = value;
					node.drawState = ds;
				}

				RefreshExpandedState(); //This should not be needed. GraphView needs to improve the extension api here
				UpdatePortInputVisibilities();
			}
		}

		void CopyToClipboard()
		{
			
		}

		public string SanitizeName(string name)
		{
			return new string(name.Where(c => !Char.IsWhiteSpace(c)).ToArray());
		}

		void UpdateSettingsExpandedState()
		{
			m_ShowSettings = !m_ShowSettings;
			if (m_ShowSettings)
			{
				m_NodeSettingsView.Add(m_Settings);
				m_NodeSettingsView.visible = true;

				m_SettingsButton.AddToClassList("clicked");
			}
			else
			{
				m_Settings.RemoveFromHierarchy();

				m_NodeSettingsView.visible = false;
				m_SettingsButton.RemoveFromClassList("clicked");
			}
		}

		void UpdateTitle()
		{
			title = node.name;
		}

		public void OnModified(ModificationScope scope)
		{
			UpdateTitle();

			base.expanded = node.drawState.expanded;

			// Update slots to match node modification
			if (scope == ModificationScope.Topological)
			{
				var slots = node.GetSlots<NodeSlot>().ToList();

				var inputPorts = inputContainer.Children().OfType<NodePort>().ToList();
				foreach (var port in inputPorts)
				{
					var currentSlot = port.slot;
					var newSlot = slots.FirstOrDefault(s => s.id == currentSlot.id);
					if (newSlot == null)
					{
						// Slot doesn't exist anymore, remove it
						inputContainer.Remove(port);

						// We also need to remove the inline input
						var portInputView = m_PortInputContainer.OfType<PortInputView>().FirstOrDefault(v => Equals(v.slot, port.slot));
						if (portInputView != null)
							portInputView.RemoveFromHierarchy();
					}
					else
					{
						port.slot = newSlot;
						var portInputView = m_PortInputContainer.OfType<PortInputView>().FirstOrDefault(x => x.slot.id == currentSlot.id);
						portInputView.UpdateSlot(newSlot);

						slots.Remove(newSlot);
					}
				}

				var outputPorts = outputContainer.Children().OfType<NodePort>().ToList();
				foreach (var port in outputPorts)
				{
					var currentSlot = port.slot;
					var newSlot = slots.FirstOrDefault(s => s.id == currentSlot.id);
					if (newSlot == null)
					{
						outputContainer.Remove(port);
					}
					else
					{
						port.slot = newSlot;
						slots.Remove(newSlot);
					}
				}

				AddSlots(slots);

				slots.Clear();
				slots.AddRange(node.GetSlots<NodeSlot>());

				if (inputContainer.childCount > 0)
					inputContainer.Sort((x, y) => slots.IndexOf(((NodePort)x).slot) - slots.IndexOf(((NodePort)y).slot));
				if (outputContainer.childCount > 0)
					outputContainer.Sort((x, y) => slots.IndexOf(((NodePort)x).slot) - slots.IndexOf(((NodePort)y).slot));
			}
			else
			{
				m_PriorityField.visible = CalculatePriorityVisibility(node);
			}

			RefreshExpandedState(); //This should not be needed. GraphView needs to improve the extension api here
			UpdatePortInputs();
			UpdatePortInputVisibilities();

			foreach (var control in m_ControlItems)
			{
				var listener = control as INodeModificationListener;
				if (listener != null)
					listener.OnNodeModified(scope);
			}
		}

		private bool CalculatePriorityVisibility(INode node)
		{
			var slots = ListPool<ISlot>.Get();
			var edges = ListPool<IEdge>.Get();
			node.GetOutputSlots(slots);
			foreach (var slot in slots)
				node.owner.GetEdges(node.GetSlotReference(slot.id), edges);
			return edges.Any(e => node.owner.GetNodeFromGuid(e.inputSlot.nodeGuid).FindSlot<ISlot>(e.inputSlot.slotId).allowMultipleConnections);
		}

		void AddSlots(IEnumerable<NodeSlot> slots)
		{
			foreach (var slot in slots)
			{
				if (slot.hidden)
					continue;

				var port = NodePort.Create(slot, m_ConnectorListener);
				if (slot.isOutputSlot)
					outputContainer.Add(port);
				else
					inputContainer.Add(port);
			}
		}

		void UpdatePortInputs()
		{
			foreach (var port in inputContainer.OfType<NodePort>())
			{
				if (!m_PortInputContainer.OfType<PortInputView>().Any(a => Equals(a.slot, port.slot)))
				{
					var portInputView = new PortInputView(port.slot)
					{
						style =
						{
							positionType = PositionType.Absolute
						}
					};
					m_PortInputContainer.Add(portInputView);
					port.RegisterCallback<GeometryChangedEvent>(evt => UpdatePortInput((NodePort)evt.target));
				}
			}
		}

		void UpdatePortInput(NodePort port)
		{
			var inputView = m_PortInputContainer.OfType<PortInputView>().First(x => Equals(x.slot, port.slot));

			var currentRect = new Rect(inputView.style.positionLeft, inputView.style.positionTop, inputView.style.width, inputView.style.height);
			var targetRect = new Rect(0.0f, 0.0f, port.layout.width, port.layout.height);
			targetRect = port.ChangeCoordinatesTo(inputView.shadow.parent, targetRect);
			var centerY = targetRect.center.y;
			var centerX = targetRect.xMax - currentRect.width;
			currentRect.center = new Vector2(centerX, centerY);

			inputView.style.positionTop = currentRect.yMin;
			var newHeight = inputView.parent.layout.height;
			foreach (var element in inputView.parent.Children())
				newHeight = Mathf.Max(newHeight, element.style.positionTop + element.layout.height);
			if (Math.Abs(inputView.parent.style.height - newHeight) > 1e-3)
				inputView.parent.style.height = newHeight;
		}

		public void UpdatePortInputVisibilities()
		{
			foreach (var portInputView in m_PortInputContainer.OfType<PortInputView>())
			{
				var slot = portInputView.slot;
				var oldVisibility = portInputView.visible;
				portInputView.visible = expanded && !node.owner.GetEdges(node.GetSlotReference(slot.id)).Any();
				if (portInputView.visible != oldVisibility)
					m_PortInputContainer.Dirty(ChangeType.Repaint);
			}
		}

		public void UpdatePortInputTypes()
		{
			foreach (var anchor in inputContainer.Concat(outputContainer).OfType<NodePort>())
			{
				var slot = anchor.slot;
				anchor.portName = slot.displayName;
				anchor.visualClass = slot.valueType.Type.Name;
			}

			foreach (var portInputView in m_PortInputContainer.OfType<PortInputView>())
				portInputView.UpdateSlotType();

			foreach (var control in m_ControlItems)
			{
				var listener = control as INodeModificationListener;
				if (listener != null)
					listener.OnNodeModified(ModificationScope.Graph);
			}
		}

		void OnResize(Vector2 deltaSize)
		{
			
		}

		void UpdateSize()
		{
			
		}

		public void Dispose()
		{
			foreach (var portInputView in m_PortInputContainer.OfType<PortInputView>())
				portInputView.Dispose();

			node = null;
		}
	}
}