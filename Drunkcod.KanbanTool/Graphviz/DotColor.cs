namespace Drunkcod.Graphviz
{
	public struct DotColor
	{
		readonly string value;

		internal DotColor(string value) {  this.value = value; }

		public static DotColor Hex(string input) => new DotColor(input);
		public static DotColor Gradient(DotColor start, DotColor end) => new DotColor(start.value + ':' + end.value);

		public override string ToString() => value ?? string.Empty;
	}
}
