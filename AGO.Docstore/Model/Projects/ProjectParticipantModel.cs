using System;
using System.ComponentModel;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Projects
{
	public class ProjectParticipantModel : DocstoreModel<Guid>
	{
		#region Persistent
		
		[DisplayName("Группа"), JsonProperty]
		public virtual string GroupName { get; set; }

		[DisplayName("Группа по умолчанию"), JsonProperty]
		public virtual bool IsDefaultGroup { get; set; }

		[DisplayName("Проект"), /*JsonProperty,*/ NotNull]
		public virtual ProjectModel Project { get; set; }

		[DisplayName("Пользователь")/*, JsonProperty,*/, NotNull]
		public virtual UserModel User { get; set; }

		#endregion
	}
}
