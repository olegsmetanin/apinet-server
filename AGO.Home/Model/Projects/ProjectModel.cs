using System;
using System.Collections.Generic;
using AGO.Core.Model;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
{
	public class ProjectModel : SecureModel<Guid>, IProjectBoundModel, IHomeModel
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

		[JsonProperty, NotNull, Prefetched]
		public virtual ProjectStatusModel Status { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? StatusId { get; set; }

		[PersistentCollection(CascadeType = CascadeType.Delete)]
		public virtual ISet<ProjectStatusHistoryModel> StatusHistory { get { return _StatusHistory; } set { _StatusHistory = value; } }
		private ISet<ProjectStatusHistoryModel> _StatusHistory = new HashSet<ProjectStatusHistoryModel>();

		[PersistentCollection(CascadeType = CascadeType.Delete)]
		public virtual ISet<ProjectParticipantModel> Participants { get { return _Participants; } set { _Participants = value; } }
		private ISet<ProjectParticipantModel> _Participants = new HashSet<ProjectParticipantModel>();

		[PersistentCollection(CascadeType = CascadeType.Delete)]
		public virtual ISet<ProjectToTagModel> Tags { get { return _Tags; } set { _Tags = value; } }
		private ISet<ProjectToTagModel> _Tags = new HashSet<ProjectToTagModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
