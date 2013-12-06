using System;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectParticipantModel : CoreModel<Guid>
	{
		#region Persistent
		
		[JsonProperty]
		public virtual string GroupName { get; set; }

		[JsonProperty]
		public virtual bool IsDefaultGroup { get; set; }

		[JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[JsonProperty, NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? UserId { get; set; }

		#endregion
	}
}
