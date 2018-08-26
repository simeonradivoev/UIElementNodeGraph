using System.Collections.Generic;
using NodeEditor.Controls;

namespace NodeEditor.Nodes
{
	public class ValueNode<T> : AbstractNode, IPropertyFromNode, IHasValue<T>,IValueSetter<T>
	{
		private const string kOutputSlotName = "Out";

		private ValueSlot<T> m_Output;

		public ValueNode()
		{
			m_Output = CreateOutputSlot<ValueSlot<T>>(kOutputSlotName);
			name = typeof(T).Name;
			UpdateNodeAfterDeserialization();
		}

		[DefaultControl]
		public T value
		{
			get { return m_Output.value; }
			set
			{
				m_Output.value = value;
				Dirty(ModificationScope.Node);
			}
		}

		public void SetValue(T val)
		{
			m_Output.value = val;
		}

		public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
		{
			properties.Add(new PreviewProperty(typeof(T))
			{
				name = GetVariableNameForNode(),
				value = value
			});
		}

		public INodeProperty AsNodeProperty()
		{
			return new ValueProperty<T> { value = value };
		}

		public int outputSlotId => 0;
	}
}