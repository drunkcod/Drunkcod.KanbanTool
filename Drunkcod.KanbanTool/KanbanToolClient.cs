using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

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
		[JsonProperty("swimlanes")] public KanbanToolSwimlane[] Swimlanes;
		[JsonProperty("workflow_stages")] public KanbanToolWorkflowStage[] WorkflowStages;

		[JsonProperty("links")] public LinkElement[] Links;

		[JsonIgnore] public string Self => Links.Single(x => x.RelMatches(nameof(Self))).Href;
		[JsonIgnore] public string Tasks => Links.Single(x => x.RelMatches(nameof(Tasks))).Href;
		[JsonIgnore] public string ArchivedTasks => Links.Single(x => x.RelMatches("archived-tasks")).Href;
		[JsonIgnore] public string Users => Links.Single(x => x.RelMatches(nameof(Users))).Href;
	}

	public class KanbanToolUser
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("name")]  public string Name;
		[JsonProperty("initials")] public string Initials;
	}

	public class KanbanToolSwimlane
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("board_id")] public int BoardId;
		[JsonProperty("name")] public string Name;
		[JsonProperty("position")] public int? Position;
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
  }

	public class KanbanToolClientRequestEventArgs : EventArgs
	{
		public readonly string RequestUri;

		public KanbanToolClientRequestEventArgs(string requestUri) { this.RequestUri = requestUri; }
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

		class UsersItem
		{
			[JsonProperty("shared_item_user")]
			public SharedItemUser Item;
		}

		class SharedItemUser
		{
			[JsonProperty("user")] public KanbanToolUser User;
		}

		public KanbanToolClient(HttpClient http)
		{
			this.http = http;
			this.json = new JsonSerializer();
		}

		public event EventHandler<KanbanToolClientRequestEventArgs> OnRequest;

		Task<Stream> GetStreamAsync(string requestUri) {
			OnRequest?.Invoke(this, new KanbanToolClientRequestEventArgs(requestUri));
			return http.GetStreamAsync(requestUri);
		}

		public async Task<IEnumerable<KanbanToolBoardResponse>> GetBoardsAsync()
		{
			var r = await GetStreamAsync("boards.json");
			using (var reader = new StreamReader(r))
				return json.Deserialize<BoardItem[]>(new JsonTextReader(reader)).Select(x => x.Item);
		}

		public async Task<KanbanToolBoardResponse> GetBoardAsync(string href)
		{
			var r = await GetStreamAsync(href);
			using (var reader = new StreamReader(r)) { 
				var board = json.Deserialize<BoardItem>(new JsonTextReader(reader)).Item;
				board.Links = new[] {
					new LinkElement("self", href),
					new LinkElement("tasks", $"boards/{board.Id}/tasks.json"),
					new LinkElement("archived-tasks", $"boards/{board.Id}/tasks.json?archived=1"),
					new LinkElement("users", $"boards/{board.Id}/users.json"),
				};
				return board;
			}
		}

		public async Task<KanbanToolUser[]> GetUsers(string href)
		{
			var r = await GetStreamAsync(href);
			using (var reader = new StreamReader(r)) {
				var users = json.Deserialize<UsersItem[]>(new JsonTextReader(reader));
				return Array.ConvertAll(users, x => x.Item.User);
			}
		}

			public async Task<KanbanToolTaskResponse> GetTasksAsync(string href)
		{
			var r = await GetStreamAsync(href);
			using (var reader = new StreamReader(r))
				return new KanbanToolTaskResponse {
					Items = json.Deserialize<KanbanToolTaskItem[]>(new JsonTextReader(reader)),
					Links = new[] {
						new LinkElement("self", href),
					}
				};
		}

		public async Task<KanbanToolTaskResponse> GetArchivedTasksAsync(string href) {
			var tasks = new List<KanbanToolTaskItem>();
			for(var n = 1;; ++n) {
				var next = href + $"&page={n}&per_page=100";
				var r = await GetStreamAsync(next);
				using (var reader = new StreamReader(r)) {
					var chunk = json.Deserialize<KanbanToolTaskItem[]>(new JsonTextReader(reader));
					if(chunk.Length == 0)
						break;
					tasks.AddRange(chunk);
				}
			}
			return new KanbanToolTaskResponse {
				Items = tasks.ToArray(),
				Links = new[] {
					new LinkElement("self", href),
				}
			};
		}
	}

	public class JsonCache
	{
		class CacheItem<T>
		{
			[JsonProperty("href")]
			public string Href;
			[JsonProperty("item")]
			public T Item;
		}

		readonly HashAlgorithm hash = SHA1.Create();
		readonly string cacheRoot;

		public JsonCache(string cacheRoot) {
			this.cacheRoot = cacheRoot;
		}

		public void AddOrUpdate<T>(string href, T value) {
			var dst = Path.Combine(cacheRoot, ToHashName(href));
			File.WriteAllText(dst, JsonConvert.SerializeObject(new CacheItem<T> { Item = value, Href = href }));
		}

		public bool TryGet<T>(string href, out T found) {
			var src = Path.Combine(cacheRoot, ToHashName(href));
			if(File.Exists(src)) { 
				found = ((CacheItem<T>)JsonConvert.DeserializeObject(File.ReadAllText(src), typeof(CacheItem<T>))).Item;
				return true;
			}
			found = default(T);
			return false;
		}

		public string ToHashName(string input) {
			var h = Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(input)));
			return h.Replace('/', '_').Replace('=', '-') + ".json";
		}
	}
}
