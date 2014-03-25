using System;
using Iesi.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Activity
{
	public class GroupedActivityItemView : ActivityItemView
	{
		private readonly ISet<string> _Users = new HashedSet<string>();
		public ISet<string> Users { get { return _Users; }}

		[JsonProperty]
		public int ChangeCount { get; set; }

		public GroupedActivityItemView(Guid itemId, string itemType, Type recordType)
			: base(itemId, itemType, recordType)
		{			
		}
	}
}