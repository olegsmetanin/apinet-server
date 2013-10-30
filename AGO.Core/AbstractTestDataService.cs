using System;
using NHibernate;

namespace AGO.Core
{
	public abstract class AbstractTestDataService : AbstractService
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;

		protected ICrudDao _CrudDao;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		protected AbstractTestDataService(
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
	}
}
