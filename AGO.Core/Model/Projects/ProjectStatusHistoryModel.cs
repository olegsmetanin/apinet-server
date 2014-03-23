using System;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectStatusHistoryModel : CoreModel<Guid>, IStatusHistoryRecordModel<ProjectModel, ProjectStatus, UserModel>
	{
		#region Persistent

		[JsonProperty, NotNull]
		public virtual UserModel Creator { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? CreatorId { get; set; }

		[JsonProperty, NotNull]
		public virtual DateTime Start { get; set; }

		[JsonProperty]
		public virtual DateTime? Finish { get; set; }

		[JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[JsonProperty]
		public virtual ProjectStatus Status { get; set; }

		#endregion

		[NotMapped]
		ProjectModel IStatusHistoryRecordModel<ProjectModel, ProjectStatus, UserModel>.Holder
		{
			get { return Project; }
			set { Project = value; }
		}

		/// <summary>
		/// Открытая запись - запись о текущем статусе объекта
		/// </summary>
		[NotMapped]
		public virtual bool IsOpen
		{
			get { return !Finish.HasValue; }
		}
	}
}