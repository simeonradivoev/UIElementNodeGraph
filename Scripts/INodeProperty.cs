using System;

namespace NodeEditor
{
	public interface INodeProperty
	{
		string displayName { get; set; }

		SerializedType propertyType { get; }
		Guid guid { get; }
		object defaultValue { get; }
		bool exposed { get; set; }

		PreviewProperty GetPreviewNodeProperty();
		INode ToConcreteNode();
		INodeProperty Copy();
	}
}