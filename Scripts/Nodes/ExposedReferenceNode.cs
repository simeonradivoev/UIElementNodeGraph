using System;
using NodeEditor.Controls;
using Object = UnityEngine.Object;

namespace NodeEditor.Nodes
{
    /// <summary>
    /// A node that can only be created by dragging from a input field of type object.
    /// This node creates a reference entry with a unique guid in it's owner if the owner is of type <see cref="IReferenceTable"/>.
    /// Then that guid is used to get and set the object reference from the <see cref="IReferenceTable"/>.
    /// When node is deleted it also deletes the reference in <see cref="IReferenceTable"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to reference.</typeparam>
	public class ExposedReferenceNode<T> : AbstractNode where T : Object
	{
        /// <summary>
        /// The guid of the exposed reference parameter. Created and stored in <see cref="IReferenceTable"/>.
        /// </summary>
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
            if (owner.owner is IReferenceTable referenceTable)
			{
                referenceTable.GetReferenceValue(guid, out var valid);
				if (!valid)
				{
					referenceTable.SetReferenceValue(guid, null);
				}
			}
		}

        /// <summary>
        /// Gets the value of the property found in <see cref="IReferenceTable"/> with the given guid.
        /// </summary>
        /// <returns></returns>
		private T ResolveValue()
		{
            if (owner.owner is IReferenceTable table)
			{
                return table.GetReferenceValue(guid, out _) as T;
			}
			return default;
		}

		public override void OnRemove()
		{
			var table = owner.owner as IReferenceTable;
			table?.ClearReferenceValue(guid);
		}
	}
}