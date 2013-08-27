using System;
using System.ComponentModel;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectStatusHistoryModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Дата начала"), JsonProperty, NotNull]
		public virtual DateTime? StartDate { get; set; }

		[DisplayName("Дата конца"), JsonProperty]
		public virtual DateTime? EndDate { get; set; }

		[DisplayName("Проект"), JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ProjectId { get; set; }

		[DisplayName("Статус"), JsonProperty, NotNull]
		public virtual ProjectStatusModel Status { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? StatusId { get; set; }

		#endregion
	}
}
