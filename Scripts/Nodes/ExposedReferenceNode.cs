using System;
using NodeEditor.Controls;
using Object = UnityEngine.Object;

namespace NodeEditor.Nodes
{
	public class ExposedReferenceNode<T> : AbstractNode where T : Object
	{
		[ExposedReferenceControl]
		public Guid propertyId => guid;

		public ExposedReferenceNode()
		{
			name = "Reference";
			CreateOutputSlot<GetterSlot<T>>("Reference").SetGetter(ResolveValue);
		}

		public override void ValidateNode()
		{
			base.ValidateNode();
			var referenceTable = owner.owner as IReferenceTable;
			if (referenceTable != null)
			{
				bool valid;
				referenceTable.GetReferenceValue(guid, out valid);
				if (!valid)
				{
					referenceTable?.SetReferenceValue(guid, null);
				}
			}
		}

		private T ResolveValue()
		{
			var table = owner.owner as IReferenceTable;
			if (table != null)
			{
				bool isValid;
				return table.GetReferenceValue(guid, out isValid) as T;
			}
			return default(T);
		}

		public override void OnRemove()
		{
			var table = owner.owner as IReferenceTable;
			table?.ClearReferenceValue(guid);
		}
	}
}