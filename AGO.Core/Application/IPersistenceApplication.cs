using System;
using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Migration;
using AGO.Core.Model.Processing;

namespace AGO.Core.Application
{
	public interface IPersistenceApplication : IApplication
	{
		ISessionProviderRegistry SessionProviderRegistry { get; }

		IFilteringService FilteringService { get; }

		DaoFactory DaoFactory { get; }

		IMigrationService MigrationService { get; }

		IModelProcessingService ModelProcessingService { get; }

		IList<Type> TestDataServices { get; }

		void CreateProjectDatabase(string connectionString, string module);
	}
}
