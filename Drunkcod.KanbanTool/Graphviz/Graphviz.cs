using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drunkcod.Graphviz
{
	public static class StringBuilderDotExtensions
	{
		public static StringBuilder AppendLine(this StringBuilder self, params DotAttribute[] attrs) =>
			self.AppendLine(string.Join(" ", attrs));

		public static StringBuilder AppendLine(this StringBuilder self, string target, params DotAttribute[] attrs) {
			self.Append(target);
			if (attrs.Length > 0)
				self.Append('[').Append(string.Join(" ", attrs)).Append(']').AppendLine();
			return self;
		}

		public static StringBuilder AppendLine(this StringBuilder self, DotNode node) =>
			self.AppendLine(node.Id, node.Attributes.ToArray());

		public static StringBuilder AppendLine(this StringBuilder self, DotEdge edge) =>
			self.AppendFormat("{0}->{1}\n", edge.From.Id, edge.To.Id);

		public static StringBuilder Cluster(this StringBuilder self, string id, IEnumerable<DotAttribute> attributes, IEnumerable<DotNode> nodes) =>
			self.Append("subgraph cluster_").Append(id)
				.Subgraph(attributes, nodes);

		public static StringBuilder Subgraph(this StringBuilder self, IEnumerable<DotAttribute> attributes, IEnumerable<DotNode> nodes) =>
			self.Append('{')
				.Append(string.Join(" ", attributes))
				.Append(string.Join(" ", nodes))
				.AppendLine("}");
	}
}
