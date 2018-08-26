using System.Collections.Generic;
using NodeEditor.Slots;
#if UNITY_EDITOR
using UnityEditor.Experimental.UIElements;
#endif
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor.Controls.Views
{
	public class LayerFieldSlotControlView : VisualElement
	{
		public LayerFieldSlotControlView(LayerSlot slot)
		{
#if UNITY_EDITOR
			var options = new List<KeyValuePair<int, string>>();
			for (int i = 0; i <= 31; i++)
			{
				var layerN = LayerMask.LayerToName(i);
				if (layerN.Length > 0)
				{
					options.Add(new KeyValuePair<int, string>(i, layerN));
				}
			}
			var layerField = new PopupField<KeyValuePair<int,string>>(options, options.FindIndex(p => p.Key == slot.value));
			layerField.OnValueChanged(e => { slot.value = e.newValue.Key; });
			Add(layerField);
#endif
		}
	}
}