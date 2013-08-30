using System;
using System.ComponentModel;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
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
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[DisplayName("Пользователь"), JsonProperty, NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? UserId { get; set; }

		#endregion
	}
}
