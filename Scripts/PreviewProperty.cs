namespace NodeEditor
{
	public struct PreviewProperty
	{
		public string name { get; set; }
		public SerializedType propType { get; private set; }

		public PreviewProperty(SerializedType type) : this()
		{
			propType = type;
		}

		private object m_value;

		public object value
		{
			get => m_value;
            set => m_value = value;
        }
	}
}