using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Drunkcod.KanbanTool
{
	public class KanbanToolBoardResponse
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("position")] public int? Position;
		[JsonProperty("name")] public string Name;
		[JsonProperty("description")] public string Description;
		[JsonProperty("created_at")] public DateTime CreatedAt;
		[JsonProperty("updated_at")] public DateTime UpdatedAt;
		[JsonProperty("last_activity_on")] public DateTime LastActivityOn;

		[JsonProperty("card_types")] public KanbanToolCardType[] CardTypes;
		[JsonProperty("workflow_stages")] public KanbanToolWorkflowStage[] WorkflowStages;

		[JsonProperty("links")] public LinkElement[] Links;
		public string Self => Links.Single(x => x.RelMatches(nameof(Self))).Href;
		public string Tasks => Links.Single(x => x.RelMatches(nameof(Tasks))).Href;
	}

	public class KanbanToolCardType
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("board_id")] public int BoardId;
		[JsonProperty("name")] public string Name;
		[JsonProperty("color_ref")] public string ColorRef;
		[JsonProperty("is_default")] public bool IsDefault;
		[JsonProperty("is_disabled")] public bool IsDisabled;
		[JsonProperty("position")] public int Position;
	}

	public class KanbanToolWorkflowStage
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("board_id")] public int BoardId;
		[JsonProperty("name")] public string Name;
		[JsonProperty("wip_limit")] public int? WipLimit;
		[JsonProperty("wip_limit_type")] public string WipLimitType;
		[JsonProperty("position")] public int Position;
		[JsonProperty("lane_width")] public int Width;
		[JsonProperty("parent_id")] public int? ParentId;
		[JsonProperty("description")] public string Description;
		[JsonProperty("lane_type_id")] public int? LaneTypeId;
		[JsonProperty("archivie_enabled")] bool CanArchive;
		[JsonProperty("lane_type")] public string LaneType;
//                "lft": 1,
 //               "rgt": 14,
 //               "board_version": 0,
  }

	public class KanbanToolClient
	{
		readonly HttpClient http;
		readonly JsonSerializer json;

		class BoardItem
		{
			[JsonProperty("board")]
			public KanbanToolBoardResponse Item;
		}

		public KanbanToolClient(HttpClient http)
		{
			this.http = http;
			this.json = new JsonSerializer();
		}

		public async Task<IEnumerable<KanbanToolBoardResponse>> GetBoardsAsync()
		{
			var r = await http.GetStreamAsync("boards.json");
			using (var reader = new StreamReader(r))
				return json.Deserialize<BoardItem[]>(new JsonTextReader(reader)).Select(x => x.Item);
		}

		public async Task<KanbanToolBoardResponse> GetBoardAsync(string href)
		{
			var r = await http.GetStreamAsync(href);
			using (var reader = new StreamReader(r)) { 
				var board = json.Deserialize<BoardItem>(new JsonTextReader(reader)).Item;
				board.Links = new[] {
					new LinkElement("self", href),
					new LinkElement("tasks", $"boards/{board.Id}/tasks.json"),
				};
				return board;
			}
		}

		public async Task<KanbanToolTaskResponse> GetTasksAsync(string href)
		{
			var r = await http.GetStreamAsync(href);
			using (var reader = new StreamReader(r))
				return new KanbanToolTaskResponse {
					Items = json.Deserialize<KanbanToolTaskItem[]>(new JsonTextReader(reader)),
					Links = new[] {
						new LinkElement("self", href),
					}
				};
		}
	}

	public class JsonCache
	{
		readonly string cacheRoot;

		public JsonCache(string cacheRoot) {
			this.cacheRoot = cacheRoot;
		}

		public void AddOrUpdate(string href, object value) {
			var dst = Path.Combine(cacheRoot, href);
			Directory.CreateDirectory(Path.GetDirectoryName(dst));
			File.WriteAllText(dst, JsonConvert.SerializeObject(value));
		}

		public T Get<T>(string href) {
			var src = Path.Combine(cacheRoot, href);
			return (T)JsonConvert.DeserializeObject(File.ReadAllText(src), typeof(T));
		}
	}
}
