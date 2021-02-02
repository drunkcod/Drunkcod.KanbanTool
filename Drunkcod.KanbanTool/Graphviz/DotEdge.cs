namespace Drunkcod.Graphviz
{
	public struct DotEdge
	{
		public readonly DotNode From;
		public readonly DotNode To;

		public DotEdge(DotNode from, DotNode to) {
			this.From = from;
			this.To = to;
		}
	}
}
