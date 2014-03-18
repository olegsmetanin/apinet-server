using System;
using System.Collections.Generic;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{

	public class ProjectModel : CoreModel<Guid>, IProjectBoundModel
	{
		public const int PROJECT_CODE_SIZE = 32;

		#region Persistent

		[JsonProperty, UniqueProperty, NotLonger(PROJECT_CODE_SIZE), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[NotLonger(64), JsonProperty, NotEmpty]
		public virtual string Name { get; set; }

		[NotLonger(512), JsonProperty]
		public virtual new string Description { get; set; }

		[JsonProperty, NotNull, Prefetched]
		public virtual ProjectTypeModel Type { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TypeId { get; set; }

		/// <summary>
		/// Project is visible in projects list for all authenticated users
		/// </summary>
		[JsonProperty]
		public virtual bool VisibleForAll { get; set; }

		[JsonProperty]
		public virtual ProjectStatus Status { get; set; }

		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
		public virtual ISet<ProjectStatusHistoryModel> StatusHistory { get { return statusHistory; } set { statusHistory = value; } }
		private ISet<ProjectStatusHistoryModel> statusHistory = new HashSet<ProjectStatusHistoryModel>();

		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
		public virtual ISet<ProjectToTagModel> Tags { get { return tags; } set { tags = value; } }
		private ISet<ProjectToTagModel> tags = new HashSet<ProjectToTagModel>();

		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
		public virtual ISet<ProjectMembershipModel> Members { get { return members; } set { members = value; } }
		private ISet<ProjectMembershipModel> members = new HashSet<ProjectMembershipModel>();

		#region Technical info

		/// <summary>
		/// Connection string to project database
		/// </summary>
		/// <remarks>May be empty, if project data stored in main db with projects and users. 
		/// Situation mostly for development and test.</remarks>
		[MetadataExclude, NotLonger(512)]
		public virtual string ConnectionString { get; set; }

		#endregion

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name.TrimSafe() ?? base.ToString();
		}

		#endregion

		public virtual ProjectStatusHistoryModel ChangeStatus(ProjectStatus newStatus, UserModel changer)
		{
			return StatusChangeHelper.Change(this, newStatus, StatusHistory, changer);
		}
	}
}