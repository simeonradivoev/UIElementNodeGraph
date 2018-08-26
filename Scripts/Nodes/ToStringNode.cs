namespace NodeEditor.Nodes
{
	[Title("Converters","To String")]
	public class ToStringNode : MethodNode<object,string>
	{
		public ToStringNode() : base("In", "Out")
		{
		}

		protected override string Execute(object input)
		{
			return input.ToString();
		}
	}
}