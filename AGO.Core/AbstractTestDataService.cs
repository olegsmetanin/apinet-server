using System;
using NHibernate;

namespace AGO.Core
{
	public abstract class AbstractTestDataService : AbstractService
	{
		#region Properties, fields, constructors

		[Obsolete("Use SessionProviderRegistry instead")]
		protected ISessionProvider _SessionProvider;

		protected ISessionProviderRegistry SessionProviderRegistry { get; private set; }

		protected ICrudDao _CrudDao;

		[Obsolete("Use SessionProviderRegistry.GetXXX.CurrentSession instead")]
		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		protected AbstractTestDataService(
			ISessionProvider sessionProvider,
			ISessionProviderRegistry registry,
			ICrudDao crudDao)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (registry == null)
				throw new ArgumentNullException("registry");
			SessionProviderRegistry = registry;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;
		}

		#endregion
	}
}
