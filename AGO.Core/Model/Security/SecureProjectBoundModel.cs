using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	/// <summary>
	/// Secure model for project bound entities, stored in project db
	/// </summary>
	public abstract class SecureProjectBoundModel<TIdType>: CoreModel<TIdType>, IProjectBoundModel, ISecureModel<ProjectMemberModel>
	{
		[JsonProperty, NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

		#region Persistent

		[NotNull]
		public virtual ProjectMemberModel Creator { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? CreatorId { get; set; }

		[JsonProperty]
		public virtual DateTime? LastChangeTime { get; set; }

		public virtual ProjectMemberModel LastChanger { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? LastChangerId { get; set; }

		#endregion
	}
}