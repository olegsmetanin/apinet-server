using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Dictionary.OrgStructure;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public class UserModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Логин"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string Login { get; set; }

		[DisplayName("MD5 хеш для авторизации в WebDav"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string PwdHash { get; set; }

		[DisplayName("Активен"), JsonProperty, NotNull]
		public virtual bool Active { get; set; }

		[DisplayName("Имя"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Фамилия"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string LastName { get; set; }

		[DisplayName("Отчество"), JsonProperty, NotLonger(64)]
		public virtual string MiddleName { get; set; }

		[DisplayName("ФИО"), JsonProperty, NotLonger(256), NotEmpty]
		public virtual string FIO { get; set; }

		[DisplayName("Фамилия с инициалами (родительный)"), JsonProperty, NotLonger(256), NotEmpty]
		public virtual string WhomFIO { get; set; }

		[DisplayName("Краткое наименование должности (именительный)"), JsonProperty, NotLonger(64)]
		public virtual string JobName { get; set; }

		[DisplayName("Краткое наименование должности (родительный)"), JsonProperty, NotLonger(64)]
		public virtual string WhomJobName { get; set; }

		[DisplayName("Группа"), JsonProperty]
		public virtual UserGroupModel Group { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? GroupId { get; set; }

		[DisplayName("Подразделения"), PersistentCollection(Inverse = false)]
		public virtual ISet<DepartmentModel> Departments { get { return _Departments; } set { _Departments = value; } }
		private ISet<DepartmentModel> _Departments = new HashSet<DepartmentModel>();

		#endregion
	}
}
