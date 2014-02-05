using System.Security.Cryptography;
using System.Text;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Model.Reporting;
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

			var artem1 = new UserModel
			{
				Creator = admin,
				Login = "artem1@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Naumov",
				Name = "Artem",
				MiddleName = "",
				SystemRole = SystemRole.Administrator
			};
			var artem1Fb = new UserModel
			{
				Creator = admin,
				Login = "artem1@facebook.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Facebook",
				Name = "Artem",
				MiddleName = "",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Facebook,
				OAuthUserId = "100007697794498"
			};
			
			var artem1Twi = new UserModel
			{
				Creator = admin,
				Login = "artem1@twitter.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Twitter",
				Name = "Artem",
				MiddleName = "",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Twitter,
				OAuthUserId = "1632745315"
			};
			_CrudDao.Store(artem1);
			_CrudDao.Store(artem1Fb);
			_CrudDao.Store(artem1Twi);
			var olegsmith = new UserModel
			{
				Creator = admin,
				Login = "olegsmith@apinet-test.com",
				PwdHash = pwdHash,
				Active = true,
				LastName = "Smith",
				Name = "Oleg",
				MiddleName = "",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Facebook,
				OAuthUserId = "1640647496"
			};
			_CrudDao.Store(olegsmith);


			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Name = "Urgent",
				FullName = "Urgent",
			});

			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Name = "Important",
				FullName = "Important",
			});

			_CrudDao.Store(new ProjectTagModel
			{
				Creator = admin,
				Name = "Pay attention",
				FullName = "Pay attention",
			});

			_CrudDao.Store(new ReportingServiceDescriptorModel
			{
			    Name = "Default",
				EndPoint = "http://localhost:36652",
				LongRunning = false
			});

			_CrudDao.Store(new ReportingServiceDescriptorModel
			{
				Name = "Long-runnign reports",
				EndPoint = "http://localhost:36652",
				LongRunning = true
			});
		}

		#endregion
	}
}
