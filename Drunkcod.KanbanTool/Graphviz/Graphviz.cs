using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drunkcod.Graphviz
{
	public class DotNode
	{
		static class NodeAttributes
		{
			public const string Label = "label";
			public const string Color = "color";
			public const string FillColor = "fillcolor";
			public const string Url = "URL";
		}

		readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
		public readonly string Id;
		public DotNode(string id) { this.Id = id; }

		public string Label {
			get { return attributes.TryGetValue(NodeAttributes.Label, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Label] = value; }
		}

		public string Color {
			get { return attributes.TryGetValue(NodeAttributes.Color, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Color] = value; }
		}

		public string FillColor {
			get { return attributes.TryGetValue(NodeAttributes.FillColor, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.FillColor] = value; }
		}

		public string Url {
			get { return attributes.TryGetValue(NodeAttributes.Url, out var found) ? found : string.Empty; }
			set { attributes[NodeAttributes.Url] = value; }
		}

		public IEnumerable<DotAttribute> Attributes => attributes.Select(x => new DotAttribute(x.Key, x.Value));

		public override string ToString() => Id;
	}

	public struct DotEdge
	{
		public readonly DotNode From;
		public readonly DotNode To;

		public DotEdge(DotNode from, DotNode to) {
			this.From = from;
			this.To = to;
		}
	}

	public struct DotAttribute
	{
		public readonly string Name;
		public readonly string Value;

		public DotAttribute(string name, string value) {
			this.Name = name;
			this.Value = value;
		}

		public override string ToString() => $"{Name}=\"{Value.Replace("\"", "\\\"")}\"";
	}

	public class DotDriver
	{
		readonly string dotPath;

		public DotDriver(string dotPath) { this.dotPath = dotPath; }

		public string Svg(string input) {
			var dot = Process.Start(new ProcessStartInfo {
				FileName = dotPath,
				Arguments = "-Tsvg",
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			});
			dot.StandardInput.WriteLine(input);
			dot.StandardInput.Close();
			var r = dot.StandardOutput.ReadToEnd();
			dot.WaitForExit();
			if(dot.ExitCode != 0)
				throw new InvalidOperationException(dot.StandardError.ReadToEnd());
			return r;
		}
	}

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
	}
}
