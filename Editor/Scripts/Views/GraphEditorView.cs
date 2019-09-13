using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditor.Scripts;
using NodeEditor.Scripts.Views;
using NodeEditor.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using GEdge = UnityEditor.Experimental.GraphView.Edge;
using Object = UnityEngine.Object;

namespace NodeEditor.Editor.Scripts.Views
{
	[Serializable]
	class FloatingWindowsLayout
	{
		public WindowDockingLayout blackboardLayout = new WindowDockingLayout();
		public Vector2 masterPreviewSize = new Vector2(400, 400);
	}

	public class GraphEditorView : VisualElement, IDisposable
	{
		public Action showInProjectRequested { get; set; }
		public Action saveRequested { get; set; }
		NodeGraphView m_GraphView;
		AbstractNodeGraph m_Graph;
		HashSet<NodeView> m_NodeViewHashSet = new HashSet<NodeView>();
		Stack<NodeView> m_NodeStack = new Stack<NodeView>();
		EdgeConnectorListener m_EdgeConnectorListener;
		SearchWindowProvider m_SearchWindowProvider;
		BlackboardProvider m_BlackboardProvider;

		const string k_FloatingWindowsLayoutKey = "UnityEditor.ShaderGraph.FloatingWindowsLayout";
		FloatingWindowsLayout m_FloatingWindowsLayout;

		public NodeGraphView graphView
		{
			get { return m_GraphView; }
		}

		public string assetName
		{
			get { return m_BlackboardProvider.assetName; }
			set
			{
				m_BlackboardProvider.assetName = value;
			}
		}

		public GraphEditorView(EditorWindow editorWindow, AbstractNodeGraph graph)
		{
			m_Graph = graph;
			styleSheets.Add(Resources.Load<StyleSheet>("Styles/GraphEditorView"));

			string serializedWindowLayout = EditorUserSettings.GetConfigValue(k_FloatingWindowsLayoutKey);
			if (!string.IsNullOrEmpty(serializedWindowLayout))
			{
				m_FloatingWindowsLayout = JsonUtility.FromJson<FloatingWindowsLayout>(serializedWindowLayout);
			}


			var toolbar = new IMGUIContainer(() =>
			{
				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton))
				{
					if (saveRequested != null)
						saveRequested();
				}
				GUILayout.Space(6);
				if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton))
				{
					if (showInProjectRequested != null)
						showInProjectRequested();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			});
			Add(toolbar);

			var content = new VisualElement { name = "content" };
			{
				m_GraphView = new NodeGraphView(m_Graph) { name = "GraphView", viewDataKey = "NodeGraphView" };
				m_GraphView.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
				m_GraphView.AddManipulator(new ContentDragger());
				m_GraphView.AddManipulator(new SelectionDragger());
				m_GraphView.AddManipulator(new RectangleSelector());
				m_GraphView.AddManipulator(new ClickSelector());
				m_GraphView.RegisterCallback<KeyDownEvent>(OnSpaceDown);
				content.Add(m_GraphView);

				m_BlackboardProvider = new BlackboardProvider(graph);
				m_GraphView.Add(m_BlackboardProvider.blackboard);

				Rect blackboardLayout = m_BlackboardProvider.blackboard.layout;
				blackboardLayout.x = 10f;
				blackboardLayout.y = 10f;
				m_BlackboardProvider.blackboard.SetPosition(blackboardLayout);

				m_GraphView.graphViewChanged = GraphViewChanged;

				RegisterCallback<GeometryChangedEvent>(ApplySerializewindowLayouts);
			}

			m_SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
			m_SearchWindowProvider.Initialize(editorWindow, m_Graph, m_GraphView);
			m_GraphView.nodeCreationRequest = (c) =>
			{
				m_SearchWindowProvider.connectedPort = null;
				SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), m_SearchWindowProvider);
			};

			m_EdgeConnectorListener = new EdgeConnectorListener(m_Graph, m_SearchWindowProvider);

			foreach (var node in graph.GetNodes<INode>())
				AddNode(node);

			foreach (var edge in graph.GetEdges())
				AddEdge(edge);

			Add(content);
		}

		void AddNode(INode node)
		{
			var nodeView = new NodeView { userData = node };
			m_GraphView.AddElement(nodeView);
			nodeView.Initialize(node as AbstractNode, m_EdgeConnectorListener);
			node.RegisterCallback(OnNodeChanged);
			nodeView.MarkDirtyRepaint();

			if (m_SearchWindowProvider.nodeNeedsRepositioning && m_SearchWindowProvider.targetSlotReference.nodeGuid.Equals(node.guid))
			{
				m_SearchWindowProvider.nodeNeedsRepositioning = false;
				foreach (var element in nodeView.inputContainer.Children().Union(nodeView.outputContainer.Children()))
				{
					var port = element as NodePort;
					if (port == null)
						continue;
					if (port.slot.slotReference.Equals(m_SearchWindowProvider.targetSlotReference))
					{
						port.RegisterCallback<GeometryChangedEvent>(RepositionNode);
						return;
					}
				}
			}
		}

		GEdge AddEdge(IEdge edge)
		{
			var sourceNode = m_Graph.GetNodeFromGuid(edge.outputSlot.nodeGuid);
			if (sourceNode == null)
			{
				Debug.LogWarning("Source node is null");
				return null;
			}
			var sourceSlot = sourceNode.FindOutputSlot<NodeSlot>(edge.outputSlot.slotId);

			var targetNode = m_Graph.GetNodeFromGuid(edge.inputSlot.nodeGuid);
			if (targetNode == null)
			{
				Debug.LogWarning("Target node is null");
				return null;
			}
			var targetSlot = targetNode.FindInputSlot<NodeSlot>(edge.inputSlot.slotId);

			var sourceNodeView = m_GraphView.nodes.ToList().OfType<NodeView>().FirstOrDefault(x => x.node == sourceNode);
			if (sourceNodeView != null)
			{
				var sourceAnchor = sourceNodeView.outputContainer.Children().OfType<NodePort>().FirstOrDefault(x => x.slot.Equals(sourceSlot));

				var targetNodeView = m_GraphView.nodes.ToList().OfType<NodeView>().FirstOrDefault(x => x.node == targetNode);
				var targetAnchor = targetNodeView.inputContainer.Children().OfType<NodePort>().FirstOrDefault(x => x.slot.Equals(targetSlot));

				var edgeView = new GEdge
				{
					userData = edge,
					output = sourceAnchor,
					input = targetAnchor
				};

				edgeView.output.Connect(edgeView);
				edgeView.input.Connect(edgeView);
				m_GraphView.AddElement(edgeView);
				sourceNodeView.RefreshPorts();
				targetNodeView.RefreshPorts();
				sourceNodeView.UpdatePortInputTypes();
				targetNodeView.UpdatePortInputTypes();

				return edgeView;
			}

			return null;
		}

		void OnNodeChanged(INode inNode, ModificationScope scope)
		{
			if (m_GraphView == null)
				return;

			var dependentNodes = new List<INode>();
			NodeUtils.CollectNodesNodeFeedsInto(dependentNodes, inNode);
			foreach (var node in dependentNodes)
			{
				var theViews = m_GraphView.nodes.ToList().OfType<NodeView>();
				var viewsFound = theViews.Where(x => x.node.guid == node.guid).ToList();
				foreach (var drawableNodeData in viewsFound)
					drawableNodeData.OnModified(scope);
			}
		}

		static void RepositionNode(GeometryChangedEvent evt)
		{
			var port = evt.target as NodePort;
			if (port == null)
				return;
			port.UnregisterCallback<GeometryChangedEvent>(RepositionNode);
			var nodeView = port.node as NodeView;
			if (nodeView == null)
				return;
			var offset = nodeView.mainContainer.WorldToLocal(port.GetGlobalCenter() + new Vector3(3f, 3f, 0f));
			var position = nodeView.GetPosition();
			position.position -= offset;
			nodeView.SetPosition(position);
			var drawState = nodeView.node.drawState;
			drawState.position = position;
			nodeView.node.drawState = drawState;
			nodeView.MarkDirtyRepaint();
			port.MarkDirtyRepaint();
		}

		GraphViewChange GraphViewChanged(GraphViewChange graphViewChange)
		{
			if (graphViewChange.edgesToCreate != null)
			{
				foreach (var edge in graphViewChange.edgesToCreate)
				{
					var leftSlot = edge.output.GetSlot();
					var rightSlot = edge.input.GetSlot();
					if (leftSlot != null && rightSlot != null)
					{
						m_Graph.owner.RegisterCompleteObjectUndo("Connect Edge");
						m_Graph.Connect(leftSlot.slotReference, rightSlot.slotReference);
					}
				}
				graphViewChange.edgesToCreate.Clear();
			}

			if (graphViewChange.movedElements != null)
			{
				foreach (var element in graphViewChange.movedElements)
				{
					var node = element.userData as INode;
					if (node == null)
						continue;

					var drawState = node.drawState;
					drawState.position = element.GetPosition();
					node.drawState = drawState;
				}
			}

			var nodesToUpdate = m_NodeViewHashSet;
			nodesToUpdate.Clear();

			if (graphViewChange.elementsToRemove != null)
			{
				m_Graph.owner.RegisterCompleteObjectUndo("Remove Elements");
				m_Graph.RemoveElements(graphViewChange.elementsToRemove.OfType<NodeView>().Select(v => (INode)v.node),
					graphViewChange.elementsToRemove.OfType<GEdge>().Select(e => (IEdge)e.userData));
				foreach (var edge in graphViewChange.elementsToRemove.OfType<GEdge>())
				{
					if (edge.input != null)
					{
                        if (edge.input.node is NodeView nodeView && m_Graph.ContainsNodeGuid(nodeView.node.guid))
							nodesToUpdate.Add(nodeView);
					}
					if (edge.output != null)
					{
                        if (edge.output.node is NodeView nodeView && m_Graph.ContainsNodeGuid(nodeView.node.guid))
							nodesToUpdate.Add(nodeView);
					}
				}
			}

            foreach (var node in nodesToUpdate)
            {
                node.OnModified(ModificationScope.Topological);
            }

            UpdateEdgeColors(nodesToUpdate);

			return graphViewChange;
		}

		void UpdateEdgeColors(HashSet<NodeView> nodeViews)
		{
			var nodeStack = m_NodeStack;
			nodeStack.Clear();
			foreach (var nodeView in nodeViews)
				nodeStack.Push(nodeView);
			while (nodeStack.Any())
			{
				var nodeView = nodeStack.Pop();
				nodeView.UpdatePortInputTypes();
				/*foreach (var anchorView in nodeView.outputContainer.Children().OfType<Port>())
				{
					foreach (var edgeView in anchorView.connections.OfType<GEdge>())
					{
						var targetSlot = edgeView.input.GetSlot();
						if (targetSlot.valueType == SlotValueType.DynamicVector || targetSlot.valueType == SlotValueType.DynamicMatrix || targetSlot.valueType == SlotValueType.Dynamic)
						{
							var connectedNodeView = edgeView.input.node as NodeView;
							if (connectedNodeView != null && !nodeViews.Contains(connectedNodeView))
							{
								nodeStack.Push(connectedNodeView);
								nodeViews.Add(connectedNodeView);
							}
						}
					}
				}*/
				foreach (var anchorView in nodeView.inputContainer.Children().OfType<Port>())
				{
					foreach (var edgeView in anchorView.connections)
					{
						var connectedNodeView = edgeView.output.node as NodeView;
						if (connectedNodeView != null && !nodeViews.Contains(connectedNodeView))
						{
							nodeStack.Push(connectedNodeView);
							nodeViews.Add(connectedNodeView);
						}
					}
				}
			}
		}

		void HandleEditorViewChanged(GeometryChangedEvent evt)
		{
            m_BlackboardProvider.blackboard.SetPosition(m_FloatingWindowsLayout.blackboardLayout.GetLayout(m_GraphView.layout));
        }

		public void HandleGraphChanges()
		{
			m_BlackboardProvider.HandleGraphChanges();

			foreach (var node in m_Graph.removedNodes)
			{
				node.UnregisterCallback(OnNodeChanged);
				var nodeView = m_GraphView.nodes.ToList().OfType<NodeView>().FirstOrDefault(p => p.node != null && p.node.guid == node.guid);
				if (nodeView != null)
				{
					nodeView.Dispose();
					nodeView.userData = null;
					m_GraphView.RemoveElement(nodeView);
				}
			}

			foreach (var node in m_Graph.addedNodes)
			{
				AddNode(node);
			}

			foreach (var node in m_Graph.pastedNodes)
			{
				var nodeView = m_GraphView.nodes.ToList().OfType<NodeView>().FirstOrDefault(p => p.node != null && p.node.guid == node.guid);
				m_GraphView.AddToSelection(nodeView);
			}

			var nodesToUpdate = m_NodeViewHashSet;
			nodesToUpdate.Clear();

			foreach (var edge in m_Graph.removedEdges)
			{
				var edgeView = m_GraphView.graphElements.ToList().OfType<GEdge>().FirstOrDefault(p => p.userData is IEdge && Equals((IEdge)p.userData, edge));
				if (edgeView != null)
				{
					var nodeView = edgeView.input.node as NodeView;
					if (nodeView != null && nodeView.node != null)
					{
						nodesToUpdate.Add(nodeView);
					}
					edgeView.output.Disconnect(edgeView);
					edgeView.input.Disconnect(edgeView);

					edgeView.output = null;
					edgeView.input = null;

					m_GraphView.RemoveElement(edgeView);
				}
			}

			foreach (var edge in m_Graph.addedEdges)
			{
				var edgeView = AddEdge(edge);
				if (edgeView != null)
					nodesToUpdate.Add((NodeView)edgeView.input.node);
			}

            foreach (var node in nodesToUpdate)
            {
                node.OnModified(ModificationScope.Topological);
                node.UpdatePortInputVisibilities();
            }

            UpdateEdgeColors(nodesToUpdate);
		}

		void OnSpaceDown(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.F1)
			{
				if (m_GraphView.selection.OfType<NodeView>().Count() == 1)
				{
					var nodeView = (NodeView)m_GraphView.selection.First();
					if (nodeView.node.documentationURL != null)
						System.Diagnostics.Process.Start(nodeView.node.documentationURL);
				}
			}
		}

		void StoreBlackboardLayoutOnGeometryChanged(GeometryChangedEvent evt)
		{
			UpdateSerializedWindowLayout();
		}

		void ApplySerializewindowLayouts(GeometryChangedEvent evt)
		{
			UnregisterCallback<GeometryChangedEvent>(ApplySerializewindowLayouts);

			if (m_FloatingWindowsLayout != null)
			{

				// Restore blackboard layout, and make sure that it remains in the view.
				Rect blackboardRect = m_FloatingWindowsLayout.blackboardLayout.GetLayout(this.layout);

				// Make sure the dimensions are sufficiently large.
				blackboardRect.width = Mathf.Clamp(blackboardRect.width, 160f, m_GraphView.contentContainer.layout.width);
				blackboardRect.height = Mathf.Clamp(blackboardRect.height, 160f, m_GraphView.contentContainer.layout.height);

				// Make sure that the positionining is on screen.
				blackboardRect.x = Mathf.Clamp(blackboardRect.x, 0f, Mathf.Max(1f, m_GraphView.contentContainer.layout.width - blackboardRect.width - blackboardRect.width));
				blackboardRect.y = Mathf.Clamp(blackboardRect.y, 0f, Mathf.Max(1f, m_GraphView.contentContainer.layout.height - blackboardRect.height - blackboardRect.height));

				// Set the processed blackboard layout.
				m_BlackboardProvider.blackboard.SetPosition(blackboardRect);
			}
			else
			{
				m_FloatingWindowsLayout = new FloatingWindowsLayout();
			}

			// After the layout is restored from the previous session, start tracking layout changes in the blackboard.
			m_BlackboardProvider.blackboard.RegisterCallback<GeometryChangedEvent>(StoreBlackboardLayoutOnGeometryChanged);

			// After the layout is restored, track changes in layout and make the blackboard have the same behavior as the preview w.r.t. docking.
			RegisterCallback<GeometryChangedEvent>(HandleEditorViewChanged);
		}

		void UpdateSerializedWindowLayout()
		{
			if (m_FloatingWindowsLayout == null)
				m_FloatingWindowsLayout = new FloatingWindowsLayout();

			m_FloatingWindowsLayout.blackboardLayout.CalculateDockingCornerAndOffset(m_BlackboardProvider.blackboard.layout, m_GraphView.layout);
			m_FloatingWindowsLayout.blackboardLayout.ClampToParentWindow();

			string serializedWindowLayout = JsonUtility.ToJson(m_FloatingWindowsLayout);
			EditorUserSettings.SetConfigValue(k_FloatingWindowsLayoutKey, serializedWindowLayout);
		}

		public void Dispose()
		{
			if (m_GraphView != null)
			{
				saveRequested = null;
				showInProjectRequested = null;
				foreach (var node in m_GraphView.Children().OfType<NodeView>())
					node.Dispose();
				m_GraphView = null;
			}
			if (m_SearchWindowProvider != null)
			{
				Object.DestroyImmediate(m_SearchWindowProvider);
				m_SearchWindowProvider = null;
			}
		}
	}
}