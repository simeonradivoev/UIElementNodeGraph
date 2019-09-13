using System.Collections.Generic;
using NodeEditor.Controls;
using NodeEditor.Util;
using UnityEngine;

namespace NodeEditor.Nodes.Math
{
	[Title("Math","Float","Cosine")]
	public class Cosine : MethodNode<float,float>
	{
		public Cosine() : base("In", "Out") { }
		protected sealed override float Execute(float x) => Mathf.Cos(x);
	}

	[Title("Math","Float", "Sine: Single")]
	public class Sine : MethodNode<float, float>
	{
		public Sine() : base("In", "Out") { }
		protected sealed override float Execute(float x) => Mathf.Sin(x);
	}

	[Title("Math","Float", "Absolute: Single")]
	public class AbsoluteFloat : MethodNode<float, float>
	{
		public AbsoluteFloat() : base("In", "Out") { }
		protected sealed override float Execute(float x) => Mathf.Abs(x);
	}

	[Title("Math","Float", "Power: Single")]
	public class PowerFloat : MethodNode<float,float, float>
	{
		public PowerFloat() : base("In","Power", "Out") { }
		protected sealed override float Execute(float x,float y) => Mathf.Pow(x,y);
	}

	[Title("Math","2D", "Angle 2D")]
	public class Angle2D : MethodNode<Vector2, Vector2, float>
	{
		public Angle2D() : base("X", "Y", "Out") { }
		protected sealed override float Execute(Vector2 x, Vector2 y) => Vector2.Angle(x,y);
	}

	[Title("Math","3D", "Angle 3D")]
	public class Angle3D : MethodNode<Vector3, Vector3, float>
	{
		public Angle3D() : base("X", "Y", "Out") { }
		protected sealed override float Execute(Vector3 x, Vector3 y) => Vector3.Angle(x, y);
	}

	[Title("Math","2D", "Normalize 2D")]
	public class Normalize2D : MethodNode<Vector2, Vector2>
	{
		public Normalize2D() : base("In", "Out") { }
		protected sealed override Vector2 Execute(Vector2 x) => x.normalized;
	}

	[Title("Math","2D", "Normalize 3D")]
	public class Normalize3D : MethodNode<Vector3, Vector3>
	{
		public Normalize3D() : base("In", "Out") { }
		protected sealed override Vector3 Execute(Vector3 x) => Vector3.Normalize(x);
	}

	[Title("Math", "Float", "Lerp: Float")]
	public class Lerp : MethodNode<float,float,float, float>
	{
		[SerializeField] private bool m_Clamped = true;
		[DefaultControl(label = "Clamped")] public bool clamped { get => m_Clamped;
            set => m_Clamped = value;
        }
		public Lerp() : base("x","y","t", "Out") { }
		protected sealed override float Execute(float x,float y,float t) => m_Clamped ? Mathf.Lerp(x,y,t) : Mathf.LerpUnclamped(x,y,t);
	}

	[Title("Math", "Float", "Clamp: Float")]
	public class ClampFLoat : AbstractNode
	{
		protected EmptySlot<float> m_Input0;
		protected ValueSlot<float> m_Input1;
		protected ValueSlot<float> m_Input2;

		public ClampFLoat()
		{
			m_Input0 = CreateInputSlot<EmptySlot<float>>(0, "val");
			m_Input1 = CreateInputSlot<ValueSlot<float>>(1, "min").SetShowControl().SetValue(0);
			m_Input2 = CreateInputSlot<ValueSlot<float>>(2, "max").SetShowControl().SetValue(1);
			CreateOutputSlot<GetterSlot<float>>(3, "Out").SetGetter(Execute);
		}

		private float Execute() => Mathf.Clamp(m_Input0[this], m_Input1[this], m_Input2[this]);
	}

	[Title("Math", "Float", "Multiply: Float")]
	public class MultiplyFloat : AbstractNode
	{
		protected ValueSlot<float> m_Input0;
		protected EmptySlot<float> m_Input1;

		public MultiplyFloat()
		{
			m_Input0 = CreateInputSlot<ValueSlot<float>>(0, "x").SetShowControl();
			m_Input1 = CreateInputSlot<EmptySlot<float>>(1, "y");
			CreateOutputSlot<GetterSlot<float>>(3, "Out").SetGetter(Execute);
		}

		private float Execute()
		{
			var list = ListPool<float>.Get();
			GetSlotValues(m_Input1,list);
			float acum = m_Input0[this];

			foreach (var f in list)
			{
				acum *= f;
			}

			ListPool<float>.Release(list);
			return acum;
		}
	}

	[Title("Math", "Float", "Divide: Float")]
	public class DivideFloat : AbstractNode
	{
		protected ValueSlot<float> m_Input0;
		protected ValueSlot<float> m_Input1;

		public DivideFloat()
		{
			m_Input0 = CreateInputSlot<ValueSlot<float>>(0, "x").SetShowControl();
			m_Input1 = CreateInputSlot<ValueSlot<float>>(1, "y").SetShowControl();
			CreateOutputSlot<GetterSlot<float>>(3, "Out").SetGetter(Execute);
		}

		private float Execute() => m_Input0[this] / m_Input1[this];
	}

}