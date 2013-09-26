using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Core.Filters;
using AGO.Core.Model.Processing;
using AGO.Core.Modules.Attributes;

namespace AGO.Core.Controllers
{
	public class DictionaryController : AbstractController
	{
		#region Properties, fields, constructors

		public DictionaryController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<CustomPropertyTypeModel> GetCustomPropertyTypes(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			return _FilteringDao.List<CustomPropertyTypeModel>(filter, new FilteringOptions
			{
				Skip = page * pageSize,
				Take = pageSize,
				Sorters = sorters
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public CustomPropertyTypeModel GetCustomPropertyType([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<CustomPropertyTypeModel, Guid>(id, dontFetchReferences);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> CustomPropertyMetadata()
		{
			return MetadataForModelAndRelations<CustomPropertyTypeModel>();
		}

		#endregion
	}
}