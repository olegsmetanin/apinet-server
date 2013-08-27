using System;
using AGO.Core.Json;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;

namespace AGO.Core.Controllers
{
	public abstract class AbstractController : AbstractService
	{
		#region Constants

		public const int DefaultPageSize = 15;

		public const int MaxPageSize = 100;

		#endregion

		#region Properties, fields, constructors

		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		protected readonly IJsonRequestService _JsonRequestService;

		protected readonly ICrudDao _CrudDao;

		protected readonly IFilteringDao _FilteringDao;

		protected AbstractController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;

			if (jsonRequestService == null)
				throw new ArgumentNullException("jsonRequestService");
			_JsonRequestService = jsonRequestService;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");
			_FilteringDao = filteringDao;
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			var initializable = _JsonService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _JsonRequestService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _CrudDao as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringDao as IInitializable;
			if (initializable != null)
				initializable.Initialize();
		}

		#endregion

		#region Helper methods

		protected FilteringOptions OptionsFromRequest(IJsonModelsRequest request)
		{
			return new FilteringOptions
			{
				Skip = request.Page * request.PageSize,
				Take = request.PageSize,
				Sorters = request.Sorters
			};
		}

		protected FilteringOptions OptionsFromRequest<TIdType>(IJsonModelRequest<TIdType> request)
		{
			return new FilteringOptions
			{
				Take = 1,
				FetchStrategy = FetchStrategy.FetchRootReferences
			};
		}

		#endregion
	}
}