using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor.Experimental.UIElements;
#endif

namespace NodeEditor.Controls.Views
{
	public class ValueSlotControlView<T> : VisualElement
	{
		NodeSlot m_Slot;
		private INode node;

		public ValueSlotControlView(ValueSlot<T> slot)
		{
			m_Slot = slot;
			if (slot is ValueSlot<bool>)
			{
				var toggle = new Toggle();
				toggle.value = ((IHasValue<bool>)slot).value;
				toggle.OnToggle(() =>
				{
					m_Slot.owner.owner.owner.RegisterCompleteObjectUndo("Boolean Change");
					if (m_Slot is IValueSetter<bool>) ((IValueSetter<bool>)m_Slot).SetValue(toggle.value);
					m_Slot.owner.Dirty(ModificationScope.Node);
				});
				Add(toggle);

			}
#if UNITY_EDITOR
			else if (slot is ValueSlot<float>)
			{
				AddControl(new FloatField(), slot);
				AddStyleSheetPath("Styles/Controls/FloatControlView");
			}
			else if (slot is ValueSlot<double>) AddControl(new DoubleField(), slot);
			else if (slot is ValueSlot<string>)
			{
				AddControl(new TextField(), slot);
				AddStyleSheetPath("Styles/Controls/StringControlView");
			}
			else if (slot is ValueSlot<int>)
			{
				AddControl(new IntegerField(), slot);
				AddStyleSheetPath("Styles/Controls/IntegerControlView");
			}
			else if (slot is ValueSlot<Color>)
			{
				AddControl(new ColorField(), slot);
				AddStyleSheetPath("Styles/Controls/ColorControlView");
			}else if (slot is ValueSlot<Gradient>)
			{
				AddControl(new GradientField(), slot);
				AddStyleSheetPath("Styles/Controls/GradientControlView");
			}
			else if (slot is ValueSlot<AnimationCurve>)
			{
				AddControl(new CurveField(), slot);
				AddStyleSheetPath("Styles/Controls/CurveControlView");
			}
			else if (slot is ValueSlot<Vector2>) Add(new MultiFloatSlotControlView(m_Slot.owner,new []{"x","y"},()=> (m_Slot as ValueSlot<Vector2>).value,v => (m_Slot as ValueSlot<Vector2>).value = v));
			else if (slot is ValueSlot<Vector3>) Add(new MultiFloatSlotControlView(m_Slot.owner, new[] { "x", "y","z" }, () => (m_Slot as ValueSlot<Vector3>).value, v => (m_Slot as ValueSlot<Vector3>).value = v));
			else if (slot is ValueSlot<Vector4>) Add(new MultiFloatSlotControlView(m_Slot.owner, new[] { "x", "y","z","w" }, () => (m_Slot as ValueSlot<Vector4>).value, v => (m_Slot as ValueSlot<Vector4>).value = v));
			else if (slot is ValueSlot<Quaternion>) Add(new MultiFloatSlotControlView(m_Slot.owner, new[] { "x", "y", "z" }, () => (m_Slot as ValueSlot<Quaternion>).value.eulerAngles, v => (m_Slot as ValueSlot<Quaternion>).value = Quaternion.Euler(v)));
			else if (slot is ValueSlot<Rect>) Add(new MultiFloatSlotControlView(m_Slot.owner, new[] { "x", "y", "w","h" }, () =>
			{
				var rect = (m_Slot as ValueSlot<Rect>).value;
				return new Vector4(rect.x,rect.y,rect.width,rect.height);
			}, v => (m_Slot as ValueSlot<Rect>).value = new Rect(v.x,v.y,v.z,v.w)));
			else
			{
				var genericType = slot.GetType().GetGenericArguments()[0];
				if (typeof(Object).IsAssignableFrom(genericType))
				{
					var objField = new ObjectField(){objectType = genericType};
					if (slot is IHasValue<Object>) objField.value = ((IHasValue<Object>)slot).value;
					objField.OnValueChanged(e =>
					{
						slot.owner.owner.owner.RegisterCompleteObjectUndo(genericType.Name + " Change");
						if (m_Slot is IValueSetter<Object>) ((IValueSetter<Object>)m_Slot).SetValue(e.newValue);
						slot.owner.Dirty(ModificationScope.Node);
					});
					Add(objField);
					AddStyleSheetPath("Styles/Controls/ObjectSlotControlView");
				}
			}
#endif
		}

		void AddControl<T1>(BaseControl<T1> field, NodeSlot slot)
		{
			if (slot is IHasValue<T1>) field.value = ((IHasValue<T1>)slot).value;
			field.OnValueChanged(OnValueChange);
			Add(field);
		}

		void AddControl<T1>(BaseTextControl<T1> field, NodeSlot slot)
		{
			if (slot is IHasValue<T1>) field.value = ((IHasValue<T1>)slot).value;
			field.OnValueChanged(OnValueChange);
			Add(field);
		}

		void OnValueChange<T1>(ChangeEvent<T1> e)
		{
			m_Slot.owner.owner.owner.RegisterCompleteObjectUndo(typeof(T1).Name + " Change");
			if (m_Slot is IValueSetter<T1>) ((IValueSetter<T1>)m_Slot).SetValue(e.newValue);
			m_Slot.owner.Dirty(ModificationScope.Node);
		}
	}
}