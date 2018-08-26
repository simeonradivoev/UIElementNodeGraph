using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Editor.Tests
{
	[TestFixture]
	public class SlotEdgeCachingTests
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Debug.unityLogger.logHandler = new ConsoleLogHandler();
		}

		[Test]
		public void UpdatesEdgeCacheOnEdgeAdd()
		{
			TestNodeGraph graph = new TestNodeGraph();

			var node0 = new TestNode();
			var node1 = new TestNode();

			graph.AddNode(node0);
			graph.AddNode(node1);

			var outSlot = node0.CreateOutputSlot<ValueSlot<Vector3>>("Out");
			var inSlot = node1.CreateInputSlot<ValueSlot<Vector3>>("In");

			graph.Connect(outSlot.slotReference, inSlot.slotReference);
			Assert.AreEqual(1,graph.GetEdges().Count);
			Assert.IsTrue(inSlot.GetSlotConnectionCache().Contains(outSlot));
			Assert.IsTrue(outSlot.GetSlotConnectionCache().Contains(inSlot));
		}

		[Test]
		public void UpdatesEdgeCacheOnEdgeRemove()
		{
			TestNodeGraph graph = new TestNodeGraph();

			var node0 = new TestNode();
			var node1 = new TestNode();

			graph.AddNode(node0);
			graph.AddNode(node1);

			var outSlot = node0.CreateOutputSlot<ValueSlot<Vector3>>("Out");
			var inSlot = node1.CreateInputSlot<ValueSlot<Vector3>>("In");

			var edge = graph.Connect(outSlot.slotReference, inSlot.slotReference);
			Assert.AreEqual(1, graph.GetEdges().Count);
			
			graph.RemoveEdge(edge);
			Assert.IsFalse(inSlot.GetSlotConnectionCache().Any());
			Assert.IsFalse(outSlot.GetSlotConnectionCache().Any());
		}

		[Test]
		public void UpdatesAllEdgeCachesOnDeserialize()
		{
			TestNodeGraph graph = new TestNodeGraph();

			var node0 = new TestNode();
			var node1 = new TestNode();

			graph.AddNode(node0);
			graph.AddNode(node1);

			var OutSlot = node0.CreateOutputSlot<ValueSlot<Vector3>>("Out");
			var InSlot = node1.CreateInputSlot<ValueSlot<Vector3>>("In");

			graph.Connect(OutSlot.slotReference, InSlot.slotReference);
			Assert.AreEqual(1, graph.GetEdges().Count);

			var serializedGraph = EditorJsonUtility.ToJson(graph, true);

			var deserialziedGraph = JsonUtility.FromJson(serializedGraph, typeof(NodeGraph)) as IGraph;
			var node0s = deserialziedGraph.GetNodeFromGuid(node0.guid);
			var node1s = deserialziedGraph.GetNodeFromGuid(node1.guid);
			var outSlotSer = node0s.GetSlots<NodeSlot>().First();
			var inSlotSer = node1s.GetSlots<NodeSlot>().First();
			Assert.IsTrue(inSlotSer.GetSlotConnectionCache().Contains(outSlotSer));
			Assert.IsTrue(outSlotSer.GetSlotConnectionCache().Contains(inSlotSer));
		}
	}
}