using AGO.Core.Filters;
using AGO.Core.Migration;
using AGO.Core.Model.Processing;

namespace AGO.Core.Application
{
	public interface IPersistenceApplication : IApplication
	{
		ISessionProvider SessionProvider { get; }

		IFilteringService FilteringService { get; }

		IFilteringDao FilteringDao { get; }

		ICrudDao CrudDao { get; }

		IMigrationService MigrationService { get; }

		IModelProcessingService ModelProcessingService { get; }
	}
}
