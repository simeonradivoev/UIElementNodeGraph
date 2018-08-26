using System.Collections.Generic;
using System.Text;
using NodeEditor.Slots;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Controls.Views
{
	public class LayerMaskSlotControlView : VisualElement
	{
		

		public LayerMaskSlotControlView(LayerMaskSlot slot)
		{
#if UNITY_EDITOR
			var maskField = new Button(){text = GetLayerName(slot.value)};
			maskField.clickable.clicked += () =>
			{
				Vector2 pos = new Vector2(0, this.layout.height);
				pos = maskField.LocalToWorld(pos);

				GenericMenu menu = new GenericMenu();
				for (int i = 0; i <= 31; i++)
				{
					int index = i;
					var layerN = LayerMask.LayerToName(i);
					if (layerN.Length > 0)
					{
						bool hasLayer = slot.value == (slot.value | (1 << i));
						menu.AddItem(new GUIContent(i + ": " + layerN), hasLayer, () =>
						{
							if (hasLayer) slot.value ^= 1 << index;
							else slot.value |= 1 << index;
							maskField.text = GetLayerName(slot.value);
							slot.owner.Dirty(ModificationScope.Node);
						});
					}
						
				}
				menu.DropDown(new Rect(pos, Vector2.zero));
			};
			Add(maskField);
#endif
		}

		private string GetLayerName(int mask)
		{
			bool hasLayerFLag = false;
			string val = "No Layers";
			for (int i = 0; i <= 31; i++)
			{
				bool hasLayer = mask == (mask | (1 << i));
				var layerN = LayerMask.LayerToName(i);
				if (hasLayer && layerN.Length > 0)
				{
					if (hasLayerFLag) return "Multiple Layers";
					hasLayerFLag = true;
					val = layerN;
				}
			}

			return val;
		}
	}
}