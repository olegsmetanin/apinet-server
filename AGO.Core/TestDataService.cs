using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Model.Security;
using AGO.Core.Model.Dictionary.Projects;

namespace AGO.Core
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		#region Properties, fields, constructors

		public TestDataService(ISessionProvider sessionProvider, ISessionProviderRegistry registry, ICrudDao crudDao)
			:base(sessionProvider, registry, crudDao)

		{
		}

		#endregion

		#region Interfaces implementation

		public void Populate()
		{
			var admin = new UserModel
			{
				Email = "admin@apinet-test.com",
				Active = true,
				LastName = "Connor",
				FirstName = "John",
				SystemRole = SystemRole.Administrator
			};
			admin.Creator = admin;
			_CrudDao.Store(admin);

			var demo = new UserModel
			{
				Creator = admin,
				Email = "demo@apinet-test.com",
				Active = true,
				LastName = "User",
				FirstName = "Demo",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(demo);

			var user1 = new UserModel
			{
				Creator = admin,
				Email = "user1@apinet-test.com",
				Active = true,
				LastName = "Bryan",
				FirstName = "Thomas",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user1);

			var user2 = new UserModel
			{
				Creator = admin,
				Email = "user2@apinet-test.com",
				Active = true,
				LastName = "Scoggins",
				FirstName = "Samuel",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user2);

			var user3 = new UserModel
			{
				Creator = admin,
				Email = "user3@apinet-test.com",
				Active = true,
				LastName = "Moore",
				FirstName = "Caroline",
				SystemRole = SystemRole.Member
			};
			_CrudDao.Store(user3);

			var artem1Fb = new UserModel
			{
				Creator = admin,
				Email = "artem1@facebook.com",
				Active = true,
				LastName = "Facebook",
				FirstName = "Artem",
				SystemRole = SystemRole.Member,
				OAuthProvider = OAuthProvider.Facebook,
				OAuthUserId = "100007697794498"
			};
			var artem1Twi = new UserModel
			{
				Creator = admin,
				Email = "artem1@twitter.com",
				Active = true,
				LastName = "Twitter",
				FirstName = "Artem",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Twitter,
				OAuthUserId = "1632745315"
			};
			_CrudDao.Store(artem1Fb);
			_CrudDao.Store(artem1Twi);

			var olegsmith = new UserModel
			{
				Creator = admin,
				Email = "olegsmith@apinet-test.com",
				Active = true,
				LastName = "Smith",
				FirstName = "Oleg",
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
		}

		#endregion
	}
}
