using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace NodeEditor.Editor.Tests
{
	[TestFixture]
	public class AbstractMaterialGraphTests
	{
		private class TestableMGraph : AbstractNodeGraph
		{
		}

		private class TestableMNode : AbstractNode
		{
		}

		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Debug.unityLogger.logHandler = new ConsoleLogHandler();
		}

		[Test]
		public void TestCanCreateMaterialGraph()
		{
			TestableMGraph graph = new TestableMGraph();
			Assert.AreEqual(0, graph.GetEdges().Count);
			Assert.AreEqual(0, graph.GetNodes<AbstractNode>().Count());
		}

		[Test]
		public void TestCanAddMaterialNodeToMaterialGraph()
		{
			TestableMGraph graph = new TestableMGraph();

			var node = new TestableMNode();
			graph.AddNode(node);
			Assert.AreEqual(0, graph.GetEdges().Count);
			Assert.AreEqual(1, graph.GetNodes<AbstractNode>().Count());
		}

		[Test]
		public void TestCanGetMaterialNodeFromMaterialGraph()
		{
			TestableMGraph graph = new TestableMGraph();

			var node = new TestableMNode();
			graph.AddNode(node);
			Assert.AreEqual(0, graph.GetEdges().Count);
			Assert.AreEqual(1, graph.GetNodes<AbstractNode>().Count());

			Assert.AreEqual(node, graph.GetNodeFromGuid(node.guid));
			Assert.AreEqual(node, graph.GetNodeFromGuid<TestableMNode>(node.guid));
		}
	}
}