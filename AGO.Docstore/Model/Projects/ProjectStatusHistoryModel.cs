using System;
using System.ComponentModel;
using AGO.Docstore.Model.Dictionary.Projects;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Projects
{
	public class ProjectStatusHistoryModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Дата начала"), JsonProperty, NotNull]
		public virtual DateTime? StartDate { get; set; }

		[DisplayName("Дата конца"), JsonProperty]
		public virtual DateTime? EndDate { get; set; }

		[DisplayName("Документ"), /*JsonProperty,*/ NotNull]
		public virtual ProjectModel Project { get; set; }

		[DisplayName("Статус"), /*JsonProperty,*/ NotNull]
		public virtual ProjectStatusModel Status { get; set; }

		#endregion
	}
}
