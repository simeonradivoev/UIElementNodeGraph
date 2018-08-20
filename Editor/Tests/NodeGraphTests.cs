using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace NodeEditor.Editor.Tests
{
	[TestFixture]
	public class NodeGraphTests
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Debug.unityLogger.logHandler = new ConsoleLogHandler();
		}

		[Test]
		public void TestCreateMaterialGraph()
		{
			var graph = new NodeGraph();

			Assert.IsNotNull(graph);

			Assert.AreEqual(0, graph.GetNodes<AbstractNode>().Count());
		}
	}
}