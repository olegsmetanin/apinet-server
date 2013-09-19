using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
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
		public void GetDocuments(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentModel>(request.Filters),
				rows = _FilteringDao.List<DocumentModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void DocumentMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<DocumentModel>());
		}

		#endregion
	}
}