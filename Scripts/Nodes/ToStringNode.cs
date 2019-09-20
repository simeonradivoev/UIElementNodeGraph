using NodeEditor.Controls;
using UnityEngine;

namespace NodeEditor.Nodes
{
	[Title("Converters","To String")]
	public class ToStringNode : MethodNode<object,string>
    {
        [SerializeField] private string m_format;

        [DefaultControl(label = "Format")]
        public string format
        {
            get => m_format;
            set => m_format = value;
        }

        public ToStringNode() : base("In", "Out")
		{
		}

		protected override string Execute(object input)
		{
            if (string.IsNullOrEmpty(m_format))
            {
                return input.ToString();
            }
			return string.Format(m_format,input);
		}
	}
}