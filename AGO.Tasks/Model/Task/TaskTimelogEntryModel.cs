using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// One line of task timelog
	/// </summary>
	/// <remarks>
	/// ProjectMemberModel used because TaskExecutor may be deleted,
	/// but his time already tracked and we don't want lost this data.
	/// </remarks>
	public class TaskTimelogEntryModel: SecureModel<Guid>
	{
		[NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		[NotNull]
		public virtual ProjectMemberModel Member { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? MemberId { get; set; }

		[NotNull, InRange(0, null, false)]
		public virtual decimal Time { get; set; }

		[NotLonger(256)]
		public virtual string Comment { get; set; }
	}
}
