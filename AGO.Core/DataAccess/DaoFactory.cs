using System;
using AGO.Core.Filters;

namespace AGO.Core.DataAccess
{
	/// <summary>
	/// Factory for creating dao objects, binded to main or project session provider
	/// </summary>
	public class DaoFactory
	{
		private readonly ISessionProviderRegistry providerRegistry;
		private readonly IFilteringService filteringService;

		public DaoFactory(ISessionProviderRegistry providerRegistry, IFilteringService filteringService)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");

			this.providerRegistry = providerRegistry;
			this.filteringService = filteringService;
		}


		private CrudDao InternalCreateCrudDao(ISessionProvider provider)
		{
			return new CrudDao(provider, filteringService);
		}

		public ICrudDao CreateCrudDao(ISessionProvider provider)
		{
			return InternalCreateCrudDao(provider);
		}

		public ICrudDao CreateMainCrudDao()
		{
			return CreateCrudDao(providerRegistry.GetMainDbProvider());
		}

		public ICrudDao CreateProjectCrudDao(string project)
		{
			return CreateCrudDao(providerRegistry.GetProjectProvider(project));
		}

		public IFilteringDao CreateFilteringDao(ISessionProvider provider)
		{
			return InternalCreateCrudDao(provider);
		}

		public IFilteringDao CreateMainFilteringDao()
		{
			return CreateFilteringDao(providerRegistry.GetMainDbProvider());
		}

		public IFilteringDao CreateProjectFilteringDao(string project)
		{
			return CreateFilteringDao(providerRegistry.GetProjectProvider(project));
		}
	}
}