using System;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	public class ActivityItemView : AbstractActivityView
	{
		[JsonProperty]
		public string User { get; set; }

		[JsonProperty]
		public string Action { get; set; }

		[JsonProperty]
		public string Before { get; set; }

		[JsonProperty]
		public string After { get; set; }

		public string AdditionalInfo { get; set; }

		public ActivityItemView(Guid itemId, string itemType, string itemName)
			:base(itemId, itemType, itemName)
		{
		}
	}
}