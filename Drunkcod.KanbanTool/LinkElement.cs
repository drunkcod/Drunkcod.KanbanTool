using System;
using Newtonsoft.Json;

namespace Drunkcod.KanbanTool
{
	public struct LinkElement
	{
		[JsonProperty("rel")] public readonly string Rel;
		[JsonProperty("href")] public readonly string Href;

		public LinkElement(string rel, string href) {
			this.Rel = rel;
			this.Href = href;
		}

		public bool RelMatches(string r) => string.Equals(Rel, r, StringComparison.InvariantCultureIgnoreCase);
	}
}
