using System;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	public class GroupedActivityItemView : ActivityItemView
	{
		[JsonProperty]
		public int ChangeCount { get; set; }

		[JsonProperty]
		public int MoreUsers { get; set; }

		public GroupedActivityItemView(Guid itemId, string itemType, string itemName)
			: base(itemId, itemType, itemName)
		{			
		}
	}
}