using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.DataAccess;
using AGO.Core.Model.Configuration;
using AGO.Core.Model.Security;
using AGO.Core.Model.Dictionary.Projects;

namespace AGO.Core
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		#region Properties, fields, constructors

		public TestDataService(ISessionProviderRegistry registry, DaoFactory factory)
			:base(registry, factory)
		{
		}

		#endregion

		#region Interfaces implementation

		public IEnumerable<string> RequiredDatabases
		{
			get { return Enumerable.Empty<string>(); }
		}

		public void Populate()
		{
			var dao = DaoFactory.CreateMainCrudDao();

			var demoServer = new DbInstanceModel
			{
				CreationTime = DateTime.UtcNow,
				Name = "Demo database server",
				Server = "localhost",
				Provider = "PostgreSQL"
			};
			dao.Store(demoServer);

			var admin = new UserModel
			{
				Email = "admin@apinet-test.com",
				Active = true,
				LastName = "Connor",
				FirstName = "John",
				SystemRole = SystemRole.Administrator
			};
			dao.Store(admin);

			var demo = new UserModel
			{
				Email = "demo@apinet-test.com",
				Active = true,
				LastName = "User",
				FirstName = "Demo",
				SystemRole = SystemRole.Member
			};
			dao.Store(demo);

			var user1 = new UserModel
			{
				Email = "user1@apinet-test.com",
				Active = true,
				LastName = "Bryan",
				FirstName = "Thomas",
				SystemRole = SystemRole.Member
			};
			dao.Store(user1);

			var user2 = new UserModel
			{
				Email = "user2@apinet-test.com",
				Active = true,
				LastName = "Scoggins",
				FirstName = "Samuel",
				SystemRole = SystemRole.Member
			};
			dao.Store(user2);

			var user3 = new UserModel
			{
				Email = "user3@apinet-test.com",
				Active = true,
				LastName = "Moore",
				FirstName = "Caroline",
				SystemRole = SystemRole.Member
			};
			dao.Store(user3);

			var artem1Fb = new UserModel
			{
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
				Email = "artem1@twitter.com",
				Active = true,
				LastName = "Twitter",
				FirstName = "Artem",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Twitter,
				OAuthUserId = "1632745315"
			};
			dao.Store(artem1Fb);
			dao.Store(artem1Twi);

			var olegsmith = new UserModel
			{
				Email = "olegsmith@apinet-test.com",
				Active = true,
				LastName = "Smith",
				FirstName = "Oleg",
				SystemRole = SystemRole.Administrator,
				OAuthProvider = OAuthProvider.Facebook,
				OAuthUserId = "1640647496"
			};
			dao.Store(olegsmith);

			var gexFb = new UserModel
			{
				Email = "gex@nightmail.ru",
				Active = true,
				LastName = "Facebook",
				FirstName = "GeX",
				SystemRole = SystemRole.Member,
				OAuthProvider = OAuthProvider.Facebook,
				OAuthUserId = "100003869729072"
			};
			dao.Store(gexFb);

			dao.Store(new ProjectTagModel
			{
				OwnerId = admin.Id,
				Name = "Urgent",
				FullName = "Urgent",
			});

			dao.Store(new ProjectTagModel
			{
				OwnerId = admin.Id,
				Name = "Important",
				FullName = "Important",
			});

			dao.Store(new ProjectTagModel
			{
				OwnerId = admin.Id,
				Name = "Pay attention",
				FullName = "Pay attention",
			});
		}

		#endregion
	}
}
