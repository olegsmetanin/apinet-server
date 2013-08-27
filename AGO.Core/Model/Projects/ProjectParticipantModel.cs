using System;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectParticipantModel : DocstoreModel<Guid>
	{
		#region Persistent
		
		[DisplayName("Группа"), JsonProperty]
		public virtual string GroupName { get; set; }

		[DisplayName("Группа по умолчанию"), JsonProperty]
		public virtual bool IsDefaultGroup { get; set; }

		[DisplayName("Проект"), JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ProjectId { get; set; }

		[DisplayName("Пользователь"), JsonProperty, NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? UserId { get; set; }

		#endregion
	}
}
