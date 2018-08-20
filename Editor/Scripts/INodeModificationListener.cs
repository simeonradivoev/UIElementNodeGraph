namespace NodeEditor.Scripts
{
	public interface INodeModificationListener
	{
		void OnNodeModified(ModificationScope scope);
	}
}