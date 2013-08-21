﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Docstore.Model.Dictionary.Projects;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Projects
{
	public class ProjectModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
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

		[DisplayName("Статус проекта"), /*JsonProperty,*/ NotNull]
		public virtual ProjectStatusModel Status { get; set; }

		[DisplayName("История статусов учета"), PersistentCollection]
		public virtual ISet<ProjectStatusHistoryModel> StatusHistory { get { return _StatusHistory; } set { _StatusHistory = value; } }
		private ISet<ProjectStatusHistoryModel> _StatusHistory = new HashSet<ProjectStatusHistoryModel>();

		[DisplayName("Участники проекта"), PersistentCollection]
		public virtual ISet<ProjectParticipantModel> Participants { get { return _Participants; } set { _Participants = value; } }
		private ISet<ProjectParticipantModel> _Participants = new HashSet<ProjectParticipantModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}