using System.Collections.Generic;
using System.Linq;

namespace Drunkcod.Graphviz
{
	public class DotNode
	{
		static class NodeAttributes
		{
			public const string Label = "label";
			public const string Color = "color";
			public const string FillColor = "fillcolor";
			public const string FontColor = "fontcolor";
			public const string Url = "URL";
			public const string Shape = "shape";
		}

		readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
		public readonly string Id;
		public DotNode(string id) { this.Id = id; }

		public string Shape {
			get { return attributes.TryGetValue(NodeAttributes.Shape, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Shape] = value; }
		}

		public string Label {
			get { return attributes.TryGetValue(NodeAttributes.Label, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Label] = value; }
		}

		public DotColor Color {
			get { return new DotColor(attributes.TryGetValue(NodeAttributes.Color, out var found) ? found : string.Empty); }
			set { attributes[NodeAttributes.Color] = value.ToString(); }
		}

		public DotColor? FontColor {
			get { return attributes.TryGetValue(NodeAttributes.FontColor, out var found) ? new DotColor(found) : new DotColor?(); }
			set { attributes[NodeAttributes.FontColor] = value.ToString(); }
		}

		public DotColor? FillColor {
			get { return attributes.TryGetValue(NodeAttributes.FillColor, out var found) ? new DotColor(found) : new DotColor?(); }
			set { attributes[NodeAttributes.FillColor] = value.ToString(); }
		}

		public string Url {
			get { return attributes.TryGetValue(NodeAttributes.Url, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Url] = value; }
		}

		public IEnumerable<DotAttribute> Attributes => attributes.Select(x => new DotAttribute(x.Key, x.Value));

		public override string ToString() => Id;
	}
}
