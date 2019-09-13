using System.Collections.Generic;
using System.Linq;
using NodeEditor.Nodes;
using NodeEditor.Util;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor.Scripts.Views
{
	public class NodeGraphView : GraphView
	{
		public AbstractNodeGraph graph { get; private set; }

		public NodeGraphView(AbstractNodeGraph graph)
		{
			this.graph = graph;
			styleSheets.Add(Resources.Load<StyleSheet>("Styles/NodeGraphView"));
			serializeGraphElements = SerializeGraphElementsImplementation;
			canPasteSerializedData = CanPasteSerializedDataImplementation;
			unserializeAndPaste = UnserializeAndPasteImplementation;
			RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
			RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
		}

		public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
		{
			var compatibleAnchors = new List<Port>();
			var startSlot = startAnchor.GetSlot();
			if (startSlot == null)
				return compatibleAnchors;

			foreach (var candidateAnchor in ports.ToList())
			{
				var candidateSlot = candidateAnchor.GetSlot();
				if (!startSlot.IsCompatibleWith(candidateSlot))
					continue;

				compatibleAnchors.Add(candidateAnchor);
			}
			return compatibleAnchors;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			if (evt.target is GraphView || evt.target is Node)
			{
				if (selection.OfType<NodeView>().Count() == 1)
				{
					evt.menu.AppendSeparator();
					evt.menu.AppendAction("Open Documentation", SeeDocumentation, SeeDocumentationStatus);
					evt.menu.AppendAction("Convert To Property", ConvertToProperty, ConvertToPropertyStatus);
				}
			}
			else if (evt.target is BlackboardField)
			{
				evt.menu.AppendAction("Delete", (e) => DeleteSelectionImplementation("Delete", AskUser.DontAskUser), (e) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}
		}

		void SeeDocumentation(DropdownMenuAction action)
		{
			var node = selection.OfType<NodeView>().First().node;
			if (node.documentationURL != null)
				System.Diagnostics.Process.Start(node.documentationURL);
		}

        DropdownMenuAction.Status SeeDocumentationStatus(DropdownMenuAction action)
		{
			if (selection.OfType<NodeView>().First().node.documentationURL == null)
				return DropdownMenuAction.Status.Disabled;
			return DropdownMenuAction.Status.Normal;
		}

        DropdownMenuAction.Status ConvertToPropertyStatus(DropdownMenuAction action)
		{
			if (selection.OfType<NodeView>().Any(v => v.node != null))
			{
				if (selection.OfType<NodeView>().Any(v => v.node is IPropertyFromNode))
					return DropdownMenuAction.Status.Normal;
				return DropdownMenuAction.Status.Disabled;
			}
			return DropdownMenuAction.Status.Hidden;
		}

		void ConvertToProperty(DropdownMenuAction action)
		{
			var selectedNodeViews = selection.OfType<NodeView>().Select(x => x.node).ToList();
			foreach (var node in selectedNodeViews)
			{
				if (!(node is IPropertyFromNode))
					continue;

				var converter = node as IPropertyFromNode;
				var prop = converter.AsNodeProperty();
				graph.AddShaderProperty(prop);

				var propNode = new PropertyNode();
				propNode.drawState = node.drawState;
				graph.AddNode(propNode);
				propNode.propertyGuid = prop.guid;

				var oldSlot = node.FindSlot<NodeSlot>(converter.outputSlotId);
				var newSlot = propNode.FindSlot<NodeSlot>(PropertyNode.OutputSlotId);

				foreach (var edge in graph.GetEdges(oldSlot.slotReference))
					graph.Connect(newSlot.slotReference, edge.inputSlot);

				graph.RemoveNode(node);
			}
		}

		string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
		{
			var nodes = elements.OfType<NodeView>().Select(x => (INode) x.node);
			var edges = elements.OfType<UnityEditor.Experimental.GraphView.Edge>().Select(x => x.userData).OfType<IEdge>();
			var properties = selection.OfType<BlackboardField>().Select(x => x.userData as INodeProperty);

			// Collect the property nodes and get the corresponding properties
			var propertyNodeGuids = nodes.OfType<PropertyNode>().Select(x => x.propertyGuid);
			var metaProperties = this.graph.properties.Where(x => propertyNodeGuids.Contains(x.guid));

			var graph = new CopyPasteGraph(this.graph.guid, nodes, edges, properties, metaProperties);
			return JsonUtility.ToJson(graph, true);
		}

		bool CanPasteSerializedDataImplementation(string serializedData)
		{
			return CopyPasteGraph.FromJson(serializedData) != null;
		}

		void UnserializeAndPasteImplementation(string operationName, string serializedData)
		{
			graph.owner.RegisterCompleteObjectUndo(operationName);
			var pastedGraph = CopyPasteGraph.FromJson(serializedData);
			this.InsertCopyPasteGraph(pastedGraph);
		}

		void DeleteSelectionImplementation(string operationName, GraphView.AskUser askUser)
		{
			foreach (var selectable in selection)
			{
				var field = selectable as BlackboardField;
				if (field != null && field.userData != null)
				{
					if (EditorUtility.DisplayDialog("Sub Graph Will Change", "If you remove a property and save the sub graph, you might change other graphs that are using this sub graph.\n\nDo you want to continue?", "Yes", "No"))
						break;
					return;
				}
			}

			graph.owner.RegisterCompleteObjectUndo(operationName);
			//graph.RemoveElements(selection.OfType<NodeView>().Where(v => !(v.node is SubGraphOutputNode)).Select(x => (INode)x.node), selection.OfType<UnityEditor.Experimental.UIElements.GraphView.Edge>().Select(x => x.userData).OfType<IEdge>());

			foreach (var selectable in selection)
			{
				var field = selectable as BlackboardField;
				if (field != null && field.userData != null)
				{
					var property = (INodeProperty)field.userData;
					graph.RemoveShaderProperty(property.guid);
				}
			}

			selection.Clear();
		}

		static void OnDragUpdatedEvent(DragUpdatedEvent e)
		{
			var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
			bool dragging = false;
			if (selection != null)
			{
				// Blackboard
				if (selection.OfType<BlackboardField>().Any())
					dragging = true;
			}
			else
			{
				// Handle unity objects
				var objects = DragAndDrop.objectReferences;
				foreach (Object obj in objects)
				{
					if (ValidateObjectForDrop(obj))
					{
						dragging = true;
						break;
					}
				}
			}

			if (dragging)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
			}
		}

		void OnDragPerformEvent(DragPerformEvent e)
		{
			Vector2 localPos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);

			var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
			if (selection != null)
			{
				// Blackboard
				if (selection.OfType<BlackboardField>().Any())
				{
					IEnumerable<BlackboardField> fields = selection.OfType<BlackboardField>();
					foreach (BlackboardField field in fields)
					{
						CreateNode(field, localPos);
					}
				}
			}
			else
			{
				// Handle unity objects
				var objects = DragAndDrop.objectReferences;
				foreach (Object obj in objects)
				{
					if (ValidateObjectForDrop(obj))
					{
						CreateNode(obj, localPos);
					}
				}
			}
		}

		static bool ValidateObjectForDrop(Object obj)
		{
			return EditorUtility.IsPersistent(obj);
		}

		void CreateNode(object obj, Vector2 nodePosition)
		{
			var blackboardField = obj as BlackboardField;
			if (blackboardField != null)
			{
				INodeProperty property = blackboardField.userData as INodeProperty;
				if (property != null)
				{
					graph.owner.RegisterCompleteObjectUndo("Drag Property");
					var node = new PropertyNode();

					var drawState = node.drawState;
					drawState.position = new Rect(nodePosition, drawState.position.size);
					node.drawState = drawState;
					graph.AddNode(node);

					node.propertyGuid = property.guid;
				}
			}
		}
	}

	public static class GraphViewExtensions
	{
		internal static void InsertCopyPasteGraph(this NodeGraphView graphView, CopyPasteGraph copyGraph)
		{
			if (copyGraph == null)
				return;

			// Make new properties from the copied graph
			foreach (INodeProperty property in copyGraph.properties)
			{
				string propertyName = graphView.graph.SanitizePropertyName(property.displayName);
				INodeProperty copiedProperty = property.Copy();
				copiedProperty.displayName = propertyName;
				graphView.graph.AddShaderProperty(copiedProperty);

				// Update the property nodes that depends on the copied node
				var dependentPropertyNodes = copyGraph.GetNodes<PropertyNode>().Where(x => x.propertyGuid == property.guid);
				foreach (var node in dependentPropertyNodes)
				{
					node.SetOwner(graphView.graph);
					node.propertyGuid = copiedProperty.guid;
				}
			}

			using (var remappedNodesDisposable = ListPool<INode>.GetDisposable())
			{
				using (var remappedEdgesDisposable = ListPool<IEdge>.GetDisposable())
				{
					var remappedNodes = remappedNodesDisposable.value;
					var remappedEdges = remappedEdgesDisposable.value;
					graphView.graph.PasteGraph(copyGraph, remappedNodes, remappedEdges);

					if (graphView.graph.guid != copyGraph.sourceGraphGuid)
					{
						// Compute the mean of the copied nodes.
						Vector2 centroid = Vector2.zero;
						var count = 1;
						foreach (var node in remappedNodes)
						{
							var position = node.drawState.position.position;
							centroid = centroid + (position - centroid) / count;
							++count;
						}

						// Get the center of the current view
						var viewCenter = graphView.contentViewContainer.WorldToLocal(graphView.layout.center);

						foreach (var node in remappedNodes)
						{
							var drawState = node.drawState;
							var positionRect = drawState.position;
							var position = positionRect.position;
							position += viewCenter - centroid;
							positionRect.position = position;
							drawState.position = positionRect;
							node.drawState = drawState;
						}
					}

					// Add new elements to selection
					graphView.ClearSelection();
					graphView.graphElements.ForEach(element =>
					{
                        if (element is UnityEditor.Experimental.GraphView.Edge edge && remappedEdges.Contains(edge.userData as IEdge))
							graphView.AddToSelection(edge);

                        if (element is NodeView nodeView && remappedNodes.Contains(nodeView.node))
							graphView.AddToSelection(nodeView);
					});
				}
			}
		}
	}
}