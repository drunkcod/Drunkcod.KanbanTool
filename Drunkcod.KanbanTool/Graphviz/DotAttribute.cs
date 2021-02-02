namespace Drunkcod.Graphviz
{
	public struct DotAttribute
	{
		public readonly string Name;
		public readonly string Value;

		public DotAttribute(string name, string value) {
			this.Name = name;
			this.Value = value;
		}

		public DotAttribute(string name, DotColor value) : this(name, value.ToString()) { }

		public override string ToString() {
			var attr = Value.StartsWith("<") ? Value : '\"' + Value.Replace("\"", "\\\"") + '\"';
			return $"{Name}={attr}";
		}
	}
}
