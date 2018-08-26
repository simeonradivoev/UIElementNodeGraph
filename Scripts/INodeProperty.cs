using System;

namespace NodeEditor
{
	public interface INodeProperty
	{
		string displayName { get; set; }
		string reference { get; set; }
		SerializedType propertyType { get; }
		Guid guid { get; }
		object defaultValue { get; }

		PreviewProperty GetPreviewNodeProperty();
		INode ToConcreteNode();
		INodeProperty Copy();
	}
}