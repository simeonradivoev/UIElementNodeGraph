using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Nodes
{
	[Title("Test Node")]
	public class TestNode : AbstractNode, IHasSettings
	{
		public TestNode()
		{
			name = "Test Node";
			int index = 0;
			AddSlot(new ValueSlot<float>(index++, "Output",SlotType.Output));
			AddSlot(new ValueSlot<float>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<bool>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<int>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Rect>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Color>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Bounds>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Vector2>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Vector3>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Vector4>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Quaternion>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Texture2D>(index++, "Input" + index, SlotType.Input));
			AddSlot(new ValueSlot<Cubemap>(index++, "Input" + index, SlotType.Input));
		}

		public VisualElement CreateSettingsElement()
		{
			return new Settings();
		}

		public class Settings : VisualElement
		{
			public Settings()
			{
				PropertySheet ps = new PropertySheet();

				ps.Add(new PropertyRow(new Label("Workflow")));

				ps.Add(new PropertyRow(new Label("Surface")));

				ps.Add(new PropertyRow(new Label("Blend")));

				ps.Add(new PropertyRow(new Label("Two Sided")));

				Add(ps);
			}
		}
	}
}