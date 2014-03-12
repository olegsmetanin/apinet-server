using System;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ActivityItemView
	{
		public Guid ItemId { get; private set; }

		[JsonProperty]
		public string ActivityTime { get; set; }

		[JsonProperty]
		public string User { get; set; }

		[JsonProperty]
		public string Action { get; set; }

		[JsonProperty]
		public string Before { get; set; }

		[JsonProperty]
		public string After { get; set; }

		public ActivityItemView(Guid itemId)
		{
			ItemId = itemId;
		}
	}
}