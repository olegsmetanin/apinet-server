using System;
using System.Collections.Generic;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;
using NHibernate;

namespace AGO.Core.Controllers.Activity
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ActivityViewUserRecord
	{
		public ProjectMemberModel Member { get; private set; }

		public Guid UserId { get { return Member.UserId; } }

		[JsonProperty]
		public string Name { get { return Member.ToStringSafe(); } }

		public IFutureValue<string> AvatarUrlFuture { get; set; }

		[JsonProperty]
		public string AvatarUrl { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is ActivityViewUserRecord))
				return false;
			return Equals(UserId, ((ActivityViewUserRecord) obj).UserId);
		}

		public override int GetHashCode()
		{
			return UserId.GetHashCode();
		}

		public ActivityViewUserRecord(ProjectMemberModel member)
		{
			if (member == null)
				throw new ArgumentNullException("member");
			Member = member;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public abstract class AbstractActivityView
	{
		private readonly ISet<ActivityViewUserRecord> _Users = new HashSet<ActivityViewUserRecord>();
		[JsonProperty]
		public ISet<ActivityViewUserRecord> Users { get { return _Users; } }

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