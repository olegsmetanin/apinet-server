using System.Collections.Generic;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.DocManagement.Model.Documents;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.DocManagement.Controllers
{
	public class DocumentsController : AbstractController
	{
		#region Properties, fields, constructors

		public DocumentsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public object GetDocuments(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentModel>(request.Filters),
				rows = _FilteringDao.List<DocumentModel>(request.Filters, OptionsFromRequest(request))
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> DocumentMetadata(JsonReader input)
		{
			return MetadataForModelAndRelations<DocumentModel>();
		}

		#endregion
	}
}