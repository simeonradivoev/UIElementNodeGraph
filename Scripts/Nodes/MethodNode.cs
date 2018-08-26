using System;

namespace NodeEditor.Nodes
{
	public abstract class MethodNode<TInput,TOutput> : AbstractNode
	{
		protected EmptySlot<TInput> m_Input;

		protected MethodNode(string x,string o)
		{
			m_Input = CreateInputSlot<EmptySlot<TInput>>(0,x);
			CreateOutputSlot<GetterSlot<TOutput>>(1, o).SetGetter(ExecuteInternal);
		}

		private TOutput ExecuteInternal() => Execute(m_Input[this]);
		protected abstract TOutput Execute(TInput input);
	}

	public abstract class MethodNode<TInput0, TInput1, TOutput> : AbstractNode
	{
		protected EmptySlot<TInput0> m_Input0;
		protected EmptySlot<TInput1> m_Input1;

		protected MethodNode(string x,string y, string o)
		{
			m_Input0 = CreateInputSlot<EmptySlot<TInput0>>(0, x);
			m_Input1 = CreateInputSlot<EmptySlot<TInput1>>(1, y);
			CreateOutputSlot<GetterSlot<TOutput>>(2, o).SetGetter(ExecuteInternal);
		}

		private TOutput ExecuteInternal() => Execute(m_Input0[this], m_Input1[this]);
		protected abstract TOutput Execute(TInput0 x, TInput1 y);
	}

	public abstract class MethodNode<TInput0, TInput1,TInput2, TOutput> : AbstractNode
	{
		protected EmptySlot<TInput0> m_Input0;
		protected EmptySlot<TInput1> m_Input1;
		protected EmptySlot<TInput2> m_Input2;

		protected MethodNode(string x, string y,string z, string o)
		{
			m_Input0 = CreateInputSlot<EmptySlot<TInput0>>(0, x);
			m_Input1 = CreateInputSlot<EmptySlot<TInput1>>(1, y);
			m_Input2 = CreateInputSlot<EmptySlot<TInput2>>(2, z);
			CreateOutputSlot<GetterSlot<TOutput>>(3, o).SetGetter(ExecuteInternal);
		}

		private TOutput ExecuteInternal() => Execute(m_Input0[this], m_Input1[this],m_Input2[this]);
		protected abstract TOutput Execute(TInput0 x, TInput1 y, TInput2 z);
	}
}