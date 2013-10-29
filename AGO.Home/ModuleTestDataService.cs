using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using NHibernate;

namespace AGO.Home
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
