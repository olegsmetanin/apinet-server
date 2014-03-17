using System;
using AGO.Core.DataAccess;

namespace AGO.Core
{
	public abstract class AbstractTestDataService : AbstractService
	{
		#region Properties, fields, constructors

		protected ISessionProviderRegistry SessionProviderRegistry { get; private set; }

		protected DaoFactory DaoFactory { get; private set; }

		protected AbstractTestDataService(
			ISessionProviderRegistry registry,
			DaoFactory factory)
		{
			if (registry == null)
				throw new ArgumentNullException("registry");
			SessionProviderRegistry = registry;

			if (factory == null)
				throw new ArgumentNullException("factory");
			DaoFactory = factory;
		}

		#endregion
	}
}
