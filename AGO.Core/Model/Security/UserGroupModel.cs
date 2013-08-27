using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public class UserGroupModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Наименование"), JsonProperty, NotEmpty, NotLonger(64)]
		public virtual string Name { get; set; }

		[DisplayName("Описание"), JsonProperty, NotLonger(512)]
		public virtual new string Description { get; set; }

		[DisplayName("Пользователи"), PersistentCollection]
		public virtual ISet<UserModel> Users { get { return _Users; } set { _Users = value; } }
		private ISet<UserModel> _Users = new HashSet<UserModel>();

		#endregion
	}
}
