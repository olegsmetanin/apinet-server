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

			_CrudDao.Store(new ProjectStatusModel
			{
				Creator = admin,
				Name = "New",
				IsInitial = true
			});

			_CrudDao.Store(new ProjectStatusModel
			{
				Creator = admin,
				Name = "Closed",
				IsFinal = true
			});

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
				Owner = admin,
				Name = "Pay attention",
				FullName = "Pay attention",
			});
		}

		#endregion
	}
}
