using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.DocManagement.Model.Documents;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;

namespace AGO.DocManagement.Controllers
{
	public class DocumentsController : AbstractController
	{
		#region Properties, fields, constructors

		public DocumentsController(
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
		public object GetDocuments(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			return new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentModel>(filter),
				rows = _FilteringDao.List<DocumentModel>(filter, new FilteringOptions
				{
					Page = page,
					Sorters = sorters
				})
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> DocumentMetadata()
		{
			return MetadataForModelAndRelations<DocumentModel>();
		}

		#endregion
	}
}