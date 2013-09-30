using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Filters;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Processing;

namespace AGO.Core.Controllers
{
	public abstract class AbstractController : AbstractService
	{
		#region Properties, fields, constructors

		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		protected readonly ICrudDao _CrudDao;

		protected readonly IFilteringDao _FilteringDao;

		protected readonly ISessionProvider _SessionProvider;

		protected readonly ILocalizationService _LocalizationService;

		protected readonly IModelProcessingService _ModelProcessingService;

		protected readonly AuthController _AuthController;

		protected AbstractController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");
			_FilteringDao = filteringDao;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (modelProcessingService == null)
				throw new ArgumentNullException("modelProcessingService");
			_ModelProcessingService = modelProcessingService;

			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;
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

			initializable = _CrudDao as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringDao as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _SessionProvider as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			_AuthController.Initialize();
		}

		#endregion

		#region Helper methods

		protected IEnumerable<IModelMetadata> MetadataForModelAndRelations<TModel>()
			where TModel : IIdentifiedModel
		{
			return MetadataForModelAndRelations(typeof (TModel));
		}

		protected IEnumerable<IModelMetadata> MetadataForModelAndRelations(Type modelType)
		{
			var result = new List<IModelMetadata>();
			var processedTypes = new HashSet<Type>();

			ProcessMetadata(modelType, result, processedTypes);

			return result;
		}

		private void ProcessMetadata(Type modelType, ICollection<IModelMetadata> result, ICollection<Type> processedTypes)
		{
			if (modelType == null || processedTypes.Contains(modelType))
				return;

			var metadata = _SessionProvider.ModelMetadata(modelType);
			if (metadata == null)
				return;

			result.Add(metadata);
			processedTypes.Add(modelType);

			foreach (var modelProperty in metadata.ModelProperties)
				ProcessMetadata(modelProperty.PropertyType, result, processedTypes);
		}

		protected TModel GetModel<TModel, TId>(TId id, bool dontFetchReferences)
			where TModel : class, IIdentifiedModel<TId>
		{
			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = id.ToStringSafe()
			});

			return _FilteringDao.List<TModel>(new[] { filter }, new FilteringOptions
			{
				PageSize = 1,
				FetchStrategy = dontFetchReferences ? FetchStrategy.DontFetchReferences : FetchStrategy.Default
			}).FirstOrDefault();
		}

		#endregion
	}
}