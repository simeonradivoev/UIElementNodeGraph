namespace NodeEditor.Editor.Scripts
{
	public interface INodeModificationListener
	{
		void OnNodeModified(ModificationScope scope);
	}
}