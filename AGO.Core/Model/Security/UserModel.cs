using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public class UserModel : SecureModel<Guid>
	{
		private const int LOGIN_SIZE_CONST = 64;
		public static readonly int LOGIN_SIZE = LOGIN_SIZE_CONST;//because const will be inlined in using classes, but not const is not compile time value

		#region Persistent

		private string lastName;
		private string name;
		private string middleName;

		[JsonProperty, NotLonger(LOGIN_SIZE_CONST), NotEmpty]
		public virtual string Login { get; set; }

		[NotLonger(128), NotEmpty, MetadataExclude]
		public virtual string PwdHash { get; set; }

		[JsonProperty, NotNull]
		public virtual bool Active { get; set; }

		[JsonProperty, NotLonger(64), NotEmpty]
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

		[JsonProperty, NotLonger(64), NotEmpty]
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

		[JsonProperty, NotLonger(64)]
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

		[JsonProperty, NotLonger(256)]
		public virtual string FullName { get; protected internal set; }

		[JsonProperty, NotLonger(256)]
		public virtual string FIO { get; protected internal set; }

		[JsonProperty, NotLonger(256)]
		public virtual string WhomFIO { get; set; }

		[JsonProperty, NotLonger(64)]
		public virtual string JobName { get; set; }

		[JsonProperty, NotLonger(64)]
		public virtual string WhomJobName { get; set; }

		[JsonProperty, NotLonger(64), NotEmpty]
		public virtual SystemRole SystemRole { get; set; }
		
		[PersistentCollection(Inverse = false)]
		public virtual ISet<DepartmentModel> Departments { get { return _Departments; } set { _Departments = value; } }
		private ISet<DepartmentModel> _Departments = new HashSet<DepartmentModel>();

		#endregion

		private void CalculateNames()
		{
			//TODO: extract from here and localize algorithm
			if (MiddleName.IsNullOrWhiteSpace())
			{
				//en style
				var ni = !Name.IsNullOrWhiteSpace() ? " " + Name.Substring(0, 1).ToUpper() + "." : null;
				FIO = ni != null ? LastName + ni : LastName;

				FullName = string.Join(" ", Name, LastName); 
			}
			else
			{
				//ru style
				var ni = !Name.IsNullOrWhiteSpace() ? " " + Name.Substring(0, 1).ToUpper() + "." : null;
				var mi = !MiddleName.IsNullOrWhiteSpace() ? MiddleName.Substring(0, 1).ToUpper() + "." : null;
				FIO = ni != null && mi != null ? LastName + ni + mi : LastName;

				FullName = string.Join(" ", LastName, Name, MiddleName);
			}
		}

		/// <summary>
		/// Временное хранилище для токена залогиненного пользователя.
		/// Используется только из-за того, что метод AuthController.CurrentUser() много где используется
		/// и менять его сигнатуру долго.
		/// TODO: хак, надо будет переделать по нормальному
		/// </summary>
		[NotMapped, MetadataExclude, JsonProperty]
		public virtual Guid Token { get; set; }
	}
}
