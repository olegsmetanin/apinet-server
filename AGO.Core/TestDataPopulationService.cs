using System;
using System.Security.Cryptography;
using System.Text;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core
{
	public class TestDataPopulationService : AbstractService, ITestDataPopulationService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ICrudDao _CrudDao;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public TestDataPopulationService(
			ISessionProvider sessionProvider,
			ICrudDao crudDao)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;
		}

		#endregion

		#region Public methods

		public void Populate()
		{
			var admin = PopulateOrgStructure();
			PopulateCustomProperties(admin);
		}

		#endregion

		#region Helper methods

		protected UserModel PopulateOrgStructure()
		{
			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes("1")));

			var admin = new UserModel
			{
				Login = "admin@agosystems.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Админов",
				Name = "Админ",
				MiddleName = "Админович",
				SystemRole = SystemRole.Administrator
			};
			admin.Creator = admin;
			_CrudDao.Store(admin);

			var primaryDepartment = new DepartmentModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Основное подразделение",
				FullName = "Основное подразделение"
			};		
			_CrudDao.Store(primaryDepartment);

			admin.Departments.Add(primaryDepartment);
			_CrudDao.Store(admin);

			var childDepartment1 = new DepartmentModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Дочернее подразделение 1",
				FullName = "Основное подразделение / Дочернее подразделение 1",
				Parent = primaryDepartment
			};
			_CrudDao.Store(childDepartment1);

			var childDepartment2 = new DepartmentModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Дочернее подразделение 2",
				FullName = "Основное подразделение / Дочернее подразделение 2",
				Parent = primaryDepartment
			};
			_CrudDao.Store(childDepartment2);

			var childDepartment3 = new DepartmentModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Дочернее подразделение 3",
				FullName = "Основное подразделение / Дочернее подразделение 3",
				Parent = primaryDepartment
			};
			_CrudDao.Store(childDepartment3);

			var user1 = new UserModel
			{
				Creator = admin,
				Login = "user1@agosystems.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Иванов",
				Name = "Иван",
				MiddleName = "Иванович",
				SystemRole = SystemRole.Member
			};
			user1.Departments.Add(childDepartment1);
			_CrudDao.Store(user1);

			var user2 = new UserModel
			{
				Creator = admin,
				Login = "user2@agosystems.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Петров",
				Name = "Петр",
				MiddleName = "Петрович",
				SystemRole = SystemRole.Member
			};
			user2.Departments.Add(childDepartment2);
			_CrudDao.Store(user2);

			var user3 = new UserModel
			{
				Creator = admin,
				Login = "user3@agosystems.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Сидоров",
				Name = "Сидор",
				MiddleName = "Сидорович",
				SystemRole = SystemRole.Member
			};
			user3.Departments.Add(childDepartment3);
			_CrudDao.Store(user3);

			return admin;
		}

		protected void PopulateCustomProperties(UserModel admin)
		{
			var stringPropertyType = new CustomPropertyTypeModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Строковый параметр",
				FullName = "Строковый параметр",
				ValueType = CustomPropertyValueType.String
			};
			_CrudDao.Store(stringPropertyType);

			var numberPropertyType = new CustomPropertyTypeModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Числовой параметр",
				FullName = "Числовой параметр",
				ValueType = CustomPropertyValueType.Number
			};
			_CrudDao.Store(numberPropertyType);

			var datePropertyType = new CustomPropertyTypeModel
			{
				ProjectCode = "TODO: move me to Home module",
				Creator = admin,
				Name = "Параметр дата",
				FullName = "Параметр дата",
				ValueType = CustomPropertyValueType.Date
			};
			_CrudDao.Store(datePropertyType);		
		}

		#endregion
	}
}
