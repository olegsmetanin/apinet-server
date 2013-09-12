using System;
using System.ComponentModel;
using AGO.Core.Model;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.System.Model
{
	public class UserFilterModel : DocstoreModel<Guid>
	{
		#region Persistent

		[DisplayName("Наименование"), NotLonger(64), JsonProperty, NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Группа"), NotLonger(64), JsonProperty, NotEmpty]
		public virtual string GroupName { get; set; }

		[DisplayName("Фильтр"), JsonProperty, NotEmpty]
		public virtual string Filter { get; set; }

		[DisplayName("Пользователь"), JsonProperty, NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? UserId { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
