using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using NHibernate;

namespace AGO.Home
{
	public class TestDataPopulationService : AbstractService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public TestDataPopulationService(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		#endregion

		#region Public methods

		public void PopulateHome()
		{
			var admin = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.SystemRole == SystemRole.Administrator).Take(1).List().FirstOrDefault();
			if (admin == null)
				throw new Exception("admin is null");

			PopulateProjects(admin);

			CurrentSession.Flush();
		}

		#endregion

		#region Helper methods

		protected void PopulateProjects(UserModel admin)
		{
			var user1 = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == "user1@agosystems.com").Take(1).List().FirstOrDefault();
			if (user1 == null)
				throw new Exception("user1 is null");

			var user2 = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == "user2@agosystems.com").Take(1).List().FirstOrDefault();
			if (user2 == null)
				throw new Exception("user2 is null");

			var user3 = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == "user3@agosystems.com").Take(1).List().FirstOrDefault();
			if (user3 == null)
				throw new Exception("user3 is null");

			var docStoreType = new ProjectTypeModel
			{
				Creator = admin, 
				Name = "Проект хранилища документов",
				Module = typeof(ProjectModel).Assembly.FullName
			};
			CurrentSession.Save(docStoreType);

			var inWorkStatus = new ProjectStatusModel
			{
				Creator = admin, 
				Name = "В работе", 
				IsInitial = true
			};
			CurrentSession.Save(inWorkStatus);

			var closedStatus = new ProjectStatusModel
			{
				Creator = admin, 
				Name = "Закрыт",
				IsFinal = true
			};
			CurrentSession.Save(closedStatus);

			var project1 = new ProjectModel
			{
				Creator = admin,
				ProjectCode = "Docs1",
				Name = "Проект хранилища документов 1",
				Description = "Описание проекта 1",
				Type = docStoreType,
				Status = inWorkStatus,

			};
			CurrentSession.Save(project1);

			var historyEntry1 = new ProjectStatusHistoryModel
			{
				Creator = admin,
				StartDate = DateTime.Now,
				Project = project1,
				Status = inWorkStatus
			};
			CurrentSession.Save(historyEntry1);

			var participant1 = new ProjectParticipantModel
			{
				User = user1,
				Project = project1
			};
			CurrentSession.Save(participant1);

			var commonTag = new ProjectTagModel
			{
				Creator = admin,
				Name = "Общий тег",
				FullName = "Общий тег",
			};
			CurrentSession.Save(commonTag);
			
			var commonTagLink = new ProjectToTagModel
			{
				Creator = admin,
				Project = project1,
				Tag = commonTag
			};
			CurrentSession.Save(commonTagLink);

			var personalTag = new ProjectTagModel
			{
				Creator = admin,
				Name = "Персональный тег",
				FullName = "Персональный тег",
				Owner = admin
			};
			CurrentSession.Save(personalTag);

			var personalTagLink = new ProjectToTagModel
			{
				Creator = admin,
				Project = project1,
				Tag = personalTag
			};
			CurrentSession.Save(personalTagLink);

			var project2 = new ProjectModel
			{
				Creator = admin,
				ProjectCode = "Docs2",
				Name = "Проект хранилища документов 2",
				Description = "Описание проекта 2",
				Type = docStoreType,
				Status = closedStatus,

			};
			CurrentSession.Save(project2);

			var historyEntry2 = new ProjectStatusHistoryModel
			{
				Creator = admin,
				StartDate = DateTime.Now,
				EndDate = DateTime.Now,
				Project = project2,
				Status = inWorkStatus
			};
			CurrentSession.Save(historyEntry2);

			var historyEntry3 = new ProjectStatusHistoryModel
			{
				Creator = admin,
				StartDate = DateTime.Now,
				Project = project2,
				Status = closedStatus
			};
			CurrentSession.Save(historyEntry3);

			var participant2 = new ProjectParticipantModel
			{
				User = user2,
				Project = project2
			};
			CurrentSession.Save(participant2);

			var participant3 = new ProjectParticipantModel
			{
				User = user3,
				Project = project2
			};
			CurrentSession.Save(participant3);
		}

		#endregion
	}
}
