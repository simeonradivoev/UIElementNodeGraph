using System;
using NodeEditor.Nodes;

namespace NodeEditor
{
	public class ValueProperty<T> : AbstractNodeProperty<T>
	{
		public override SerializedType propertyType => typeof(T);

		public override object defaultValue => default(T);

		public override PreviewProperty GetPreviewNodeProperty()
		{
			return new PreviewProperty(propertyType) {name = displayName,value = defaultValue};
		}

		public override INodeProperty Copy()
		{
			var prop = (INodeProperty)Activator.CreateInstance(GetType());
			prop.displayName = displayName;
			return prop;
		}

		public override INode ToConcreteNode()
		{
			return new ValueNode<T>(){name = displayName,value = value};
		}
	}
}