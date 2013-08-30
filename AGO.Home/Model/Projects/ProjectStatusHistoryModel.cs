using System;
using System.ComponentModel;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
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
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[DisplayName("Статус"), JsonProperty, NotNull]
		public virtual ProjectStatusModel Status { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? StatusId { get; set; }

		#endregion
	}
}
