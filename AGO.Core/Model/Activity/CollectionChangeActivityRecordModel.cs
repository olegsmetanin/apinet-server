using System;
using AGO.Core.Attributes.Constraints;

namespace AGO.Core.Model.Activity
{
	public enum ChangeType
	{
		Insert,
		Delete,
		Update
	}

	public class CollectionChangeActivityRecordModel : ActivityRecordModel
	{
		#region Persistent

		[NotEmpty]
		public virtual Guid RelatedItemId { get; set; }

		[NotEmpty, NotLonger(128)]
		public virtual string RelatedItemType { get; set; }

		[NotEmpty]
		public virtual string RelatedItemName { get; set; }

		[NotEmpty]
		public virtual ChangeType ChangeType { get; set; }

		#endregion
	}
}
