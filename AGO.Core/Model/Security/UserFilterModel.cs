using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	/// <summary>
	/// Stored filter
	/// May be used in core context (for projects filter, ex.) or in project, so, ProjectCode is nullable
	/// and user OwnerId instead of direct UserModel ref
	/// </summary>
	[MetadataExclude]
	public class UserFilterModel : CoreModel<Guid>, IProjectBoundModel
	{
		#region Persistent

		[JsonProperty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

		[NotLonger(64), JsonProperty, NotEmpty]
		public virtual string Name { get; set; }

		[NotLonger(64), JsonProperty, NotEmpty]
		public virtual string GroupName { get; set; }

		[JsonProperty, NotEmpty]
		public virtual string Filter { get; set; }

		[JsonProperty, NotNull]
		public virtual Guid OwnerId { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name + (!ProjectCode.IsNullOrEmpty() ? "(" + ProjectCode + ")" : string.Empty);
		}

		#endregion
	}
}
