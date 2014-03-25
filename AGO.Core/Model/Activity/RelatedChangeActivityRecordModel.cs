using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;

namespace AGO.Core.Model.Activity
{
	public enum ChangeType
	{
		Insert,
		Delete,
		Update
	}

	public class RelatedChangeActivityRecordModel : ActivityRecordModel
	{
		#region Persistent

		[NotEmpty]
		public virtual Guid RelatedItemId { get; set; }

		[NotEmpty, NotLonger(128), MetadataExclude]
		public virtual string RelatedItemType { get; set; }

		[NotEmpty]
		public virtual string RelatedItemName { get; set; }

		[NotEmpty]
		public virtual ChangeType ChangeType { get; set; }

		#endregion
	}
}
