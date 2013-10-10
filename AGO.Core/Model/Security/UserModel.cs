using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public class UserModel : SecureModel<Guid>
	{
		#region Persistent

		private string lastName;
		private string name;
		private string middleName;

		[DisplayName("Логин"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string Login { get; set; }

		[DisplayName("MD5 хеш"), NotLonger(128), NotEmpty, MetadataExclude]
		public virtual string PwdHash { get; set; }

		[DisplayName("Активен"), JsonProperty, NotNull]
		public virtual bool Active { get; set; }

		[DisplayName("Имя"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string Name
		{
			get { return name; }
			set
			{
				if (name == value) return;

				name = value;
				CalculateNames();
			}
		}

		[DisplayName("Фамилия"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual string LastName
		{
			get { return lastName; }
			set
			{
				if (lastName == value) return;

				lastName = value;
				CalculateNames();
			}
		}

		[DisplayName("Отчество"), JsonProperty, NotLonger(64)]
		public virtual string MiddleName
		{
			get { return middleName; } 
			set
			{
				if (middleName == value) return;

				middleName = value;
				CalculateNames();
			}
		}

		[DisplayName("ФИО"), JsonProperty, NotLonger(256)]
		public virtual string FullName { get; protected internal set; }

		[DisplayName("Фамилия с инициалами (именительный)"), JsonProperty, NotLonger(256)]
		public virtual string FIO { get; protected internal set; }

		[DisplayName("Фамилия с инициалами (родительный)"), JsonProperty, NotLonger(256)]
		public virtual string WhomFIO { get; set; }

		[DisplayName("Краткое наименование должности (именительный)"), JsonProperty, NotLonger(64)]
		public virtual string JobName { get; set; }

		[DisplayName("Краткое наименование должности (родительный)"), JsonProperty, NotLonger(64)]
		public virtual string WhomJobName { get; set; }

		[DisplayName("Группа"), JsonProperty, NotLonger(64), NotEmpty]
		public virtual SystemRole SystemRole { get; set; }
		
		[DisplayName("Подразделения"), PersistentCollection(Inverse = false)]
		public virtual ISet<DepartmentModel> Departments { get { return _Departments; } set { _Departments = value; } }
		private ISet<DepartmentModel> _Departments = new HashSet<DepartmentModel>();

		#endregion

		private void CalculateNames()
		{
			FullName =  string.Join(" ", LastName, Name, MiddleName);

			var ni = !Name.IsNullOrWhiteSpace() ? " " + Name.Substring(0, 1).ToUpper() + "." : null;
			var mi = !MiddleName.IsNullOrWhiteSpace() ? MiddleName.Substring(0, 1).ToUpper() + "." : null;
			FIO = ni != null && mi != null ? LastName + ni + mi : LastName;
		}
	}
}
