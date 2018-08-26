using UnityEngine;

namespace NodeEditor.Nodes.Builder
{
	[Title("Builder","Builder: Vector2")]
	public class Vector2ConstructorNode : AbstractNode
	{
		private readonly ValueSlot<float> m_X;
		private readonly ValueSlot<float> m_Y;

		public Vector2ConstructorNode()
		{
			name = "Vector2";
			m_X = CreateInputSlot<ValueSlot<float>>("x").SetShowControl();
			m_Y = CreateInputSlot<ValueSlot<float>>("y").SetShowControl();
			CreateOutputSlot<GetterSlot<Vector2>>("Out").SetGetter(Build);
		}

		private Vector2 Build()
		{
			return new Vector2(m_X[this], m_Y[this]);
		}
	}
}