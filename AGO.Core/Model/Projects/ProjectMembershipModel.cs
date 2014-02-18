using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Projects
{
	[RelationalModel]
	public class ProjectMembershipModel: IdentifiedModel<Guid>
	{
		[NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? UserId { get; set; }
	}
}