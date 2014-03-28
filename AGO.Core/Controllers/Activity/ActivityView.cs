using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	public class ActivityView : AbstractActivityView
	{
		[JsonProperty]
		public string ActivityItem { get; set; }

		[JsonProperty]
		public IList<ActivityItemView> Items { get; set; }

		public ActivityView(Guid itemId, string itemType, string itemName)
			:base(itemId, itemType, itemName)
		{
			Items = new List<ActivityItemView>();
		}
	}
}