
namespace NodeEditor
{
	public class NodeGraph : AbstractNodeGraph, INodeGraph
	{
		public void LoadedFromDisk()
		{
			OnEnable();
			ValidateGraph();
		}
	}
}