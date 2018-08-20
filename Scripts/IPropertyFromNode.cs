
namespace NodeEditor
{
	public interface IPropertyFromNode
	{
		INodeProperty AsNodeProperty();
		int outputSlotId { get; }
	}
}