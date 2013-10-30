using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;

namespace AGO.Home
{
	public class TestDataService : AbstractTestDataService, ITestDataService
	{
		#region Properties, fields, constructors

		public TestDataService(ISessionProvider sessionProvider, ICrudDao crudDao)
			: base(sessionProvider, crudDao)
		{
		}

		#endregion

		#region Interfaces implementation

		public void Populate()
		{
			var admin = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.SystemRole == SystemRole.Administrator).Take(1).List().FirstOrDefault();
			if (admin == null)
				throw new Exception("Test data inconsistency");

			var newProjectStatus = new ProjectStatusModel
			{
				Creator = admin,
				Name = "New",
				IsInitial = true
			};
			_CrudDao.Store(newProjectStatus);

			var closedProjectStatus = new ProjectStatusModel
			{
				Creator = admin,
				Name = "Closed",
				IsFinal = true
			};
			_CrudDao.Store(closedProjectStatus);

			var commonTag = new ProjectTagModel
			{
				Creator = admin,
				Name = "Common tag",
				FullName = "Common tag",
			};
			_CrudDao.Store(commonTag);
		}

		#endregion
	}
}
