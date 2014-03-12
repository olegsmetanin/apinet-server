using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Activity
{
	[TablePerSubclass("ModelType")]
	public abstract class ActivityRecordModel : SecureProjectBoundModel<Guid>
	{
		#region Persistent

		[NotEmpty]
		public virtual Guid ItemId { get; set; }

		[NotEmpty, NotLonger(128)]
		public virtual string ItemType { get; set; }

		[NotEmpty]
		public virtual string ItemName { get; set; }

		#endregion
	}
}
