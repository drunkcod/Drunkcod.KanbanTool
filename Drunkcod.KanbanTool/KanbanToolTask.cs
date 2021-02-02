using Newtonsoft.Json;
using System;
using System.Linq;

namespace Drunkcod.KanbanTool
{
	public class KanbanToolTaskResponse
	{
		[JsonProperty("items")] public KanbanToolTaskItem[] Items;
		[JsonProperty("links")] public LinkElement[] Links;

		public string Self => Links.Single(x => x.RelMatches(nameof(Self))).Href;
	}

	public class KanbanToolPaginiationResponse
	{

		public int Page;
	}

	public class KanbanToolTaskItem
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("board_id")] public int BoardId;
		[JsonProperty("workflow_stage_id")] public int WorkflowStageId;
		[JsonProperty("name")] public string Name;
		[JsonProperty("description")] public string Description;
		[JsonProperty("tags")] public string Tags;
		[JsonProperty("card_type_id")] public int CardTypeId;
		[JsonProperty("swimlane_id")] public int SwimlaneId;
		[JsonProperty("created_by_id")] public int CreatedById;
		[JsonProperty("archived_at")] public DateTime? ArchivedAt;
		[JsonProperty("deleted_at")] public DateTime? DeletedAt;
		[JsonProperty("assigned_user_id")] public int? AssignedUserId;
		[JsonProperty("block_reason")] public string BlockReason;
		[JsonProperty("due_date")] public DateTime? DueDate;

		[JsonProperty("custom_field_1")] public string CustomField1;
	}
}
