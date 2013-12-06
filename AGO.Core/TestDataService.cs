using System.Security.Cryptography;
using System.Text;
using AGO.Core.Model.Security;
using AGO.Core.Model.Dictionary.Projects;

namespace AGO.Core
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		#region Properties, fields, constructors

		public TestDataService(ISessionProvider sessionProvider, ICrudDao crudDao)
			:base(sessionProvider, crudDao)

		{
		}

		#endregion

		#region Interfaces implementation

		public void Populate()
		{
			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes("1")));

			var admin = new UserModel
			{
				Login = "admin@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Connor",
				Name = "John",
				MiddleName = "",
				JobName = "Administrator",
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
				LastName = "Bryan",
				Name = "Thomas",
				MiddleName = "",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user1);

			var user2 = new UserModel
			{
				Creator = admin,
				Login = "user2@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Scoggins",
				Name = "Samuel",
				MiddleName = "",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user2);

			var user3 = new UserModel
			{
				Creator = admin,
				Login = "user3@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Moore",
				Name = "Caroline",
				MiddleName = "",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user3);

			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Owner = admin,
				Name = "Urgent",
				FullName = "Urgent",
			});

			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Owner = admin,
				Name = "Important",
				FullName = "Important",
			});

			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Owner = admin,
				Name = "Pay attention",
				FullName = "Pay attention",
			});
		}

		#endregion
	}
}
