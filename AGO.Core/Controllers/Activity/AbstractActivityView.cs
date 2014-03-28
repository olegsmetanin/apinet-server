using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class AbstractActivityView
	{
		[JsonProperty]
		public Guid ItemId { get; private set; }

		[JsonProperty]
		public string ItemName { get; private set; }
		
		[JsonProperty]
		public string ItemType { get; private set; }

		[JsonProperty]
		public string ActivityTime { get; set; }

		private readonly ISet<IActivityViewProcessor> _ApplicableProcessors = new HashSet<IActivityViewProcessor>();
		public ISet<IActivityViewProcessor> ApplicableProcessors { get { return _ApplicableProcessors; } }

		protected AbstractActivityView(Guid itemId, string itemType, string itemName)
		{
			ItemId = itemId;
			ItemType = itemType;
			ItemName = itemName;
		}
	}
}