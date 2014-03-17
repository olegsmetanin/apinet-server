using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ActivityView
	{
		[JsonProperty]
		public Guid ItemId { get; private set; }

		public string ItemType { get; private set; }

		[JsonProperty]
		public string ActivityTime { get; set; }

		[JsonProperty]
		public string ActivityItem { get; set; }

		[JsonProperty]
		public bool HasDetails { get; set; }

		[JsonProperty]
		public IList<ActivityItemView> Items { get; set; }

		public ActivityView(Guid itemId, string itemType, bool hasDetails)
		{
			ItemId = itemId;
			ItemType = itemType;
			HasDetails = hasDetails;
			Items = new List<ActivityItemView>();
		}
	}
}