using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
{
	public class ProjectModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32)]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), NotEmpty, NotLonger(64), JsonProperty]
		public virtual string Name { get; set; }

		[DisplayName("Описание"), NotLonger(512), JsonProperty]
		public virtual new string Description { get; set; }

		[DisplayName("Архив"), JsonProperty]
		public virtual bool IsArchive { get; set; }

		[DisplayName("Горизонт событий проекта"), JsonProperty]
		public virtual DateTime? EventsHorizon { get; set; }

		[DisplayName("Путь к проекту"), NotLonger(512), JsonProperty]
		public virtual string FileSystemPath { get; set; }

		[DisplayName("Тип проекта"), JsonProperty, NotNull]
		public virtual ProjectTypeModel Type { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TypeId { get; set; }

		[DisplayName("Статус проекта"), JsonProperty, NotNull]
		public virtual ProjectStatusModel Status { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? StatusId { get; set; }

		[DisplayName("История статусов учета"), PersistentCollection]
		public virtual ISet<ProjectStatusHistoryModel> StatusHistory { get { return _StatusHistory; } set { _StatusHistory = value; } }
		private ISet<ProjectStatusHistoryModel> _StatusHistory = new HashSet<ProjectStatusHistoryModel>();

		[DisplayName("Участники проекта"), PersistentCollection]
		public virtual ISet<ProjectParticipantModel> Participants { get { return _Participants; } set { _Participants = value; } }
		private ISet<ProjectParticipantModel> _Participants = new HashSet<ProjectParticipantModel>();

		[DisplayName("Теги проекта"), PersistentCollection]
		public virtual ISet<ProjectTagModel> Tags { get { return _Tags; } set { _Tags = value; } }
		private ISet<ProjectTagModel> _Tags = new HashSet<ProjectTagModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
