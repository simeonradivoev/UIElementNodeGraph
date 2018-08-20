using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Nodes
{
	public class ValueNode<T> : AbstractNode, IPropertyFromNode, IHasValue<T>,IValueSetter<T>
	{
		public const int OutputSlotId = 0;
		private const string kOutputSlotName = "Out";

		public ValueNode()
		{
			name = typeof(T).Name;
			UpdateNodeAfterDeserialization();
		}

		[SerializeField]
		T m_Value;

		[Control]
		public T value
		{
			get { return m_Value; }
			set
			{
				m_Value = value;
				Dirty(ModificationScope.Node);
			}
		}

		public void SetValue(T val)
		{
			m_Value = val;
		}

		public sealed override void UpdateNodeAfterDeserialization()
		{
			AddSlot(new ValueSlot<T>(OutputSlotId, kOutputSlotName, SlotType.Output));
			RemoveSlotsNameNotMatching(new[] { OutputSlotId });
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

		public int outputSlotId { get { return OutputSlotId; } }
	}
}