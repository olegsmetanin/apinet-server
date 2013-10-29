using System;
using System.Security.Cryptography;
using System.Text;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core
{
	public class ModuleTestDataService : AbstractService, IModuleTestDataService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ICrudDao _CrudDao;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public ModuleTestDataService(
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
			DoPopulateUsers();
		}

		#endregion

		#region Helper methods

		protected void DoPopulateUsers()
		{
			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes("1")));

			var admin = new UserModel
			{
				Login = "admin@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Admin",
				Name = "Admin",
				MiddleName = "Admin",
				SystemRole = SystemRole.Administrator
			};
			admin.Creator = admin;
			_CrudDao.Store(admin);

			var user1 = new UserModel
			{
				Creator = admin,
				Login = "user1@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "User1",
				Name = "User1",
				MiddleName = "User1",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user1);

			var user2 = new UserModel
			{
				Creator = admin,
				Login = "user2@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "User2",
				Name = "User2",
				MiddleName = "User2",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user2);

			var user3 = new UserModel
			{
				Creator = admin,
				Login = "user3@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "User3",
				Name = "User3",
				MiddleName = "User3",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user3);
		}

		#endregion
	}
}
