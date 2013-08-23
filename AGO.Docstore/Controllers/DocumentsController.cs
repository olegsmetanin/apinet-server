using AGO.Docstore.Json;
using AGO.Docstore.Model.Documents;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using AGO.Hibernate.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Docstore.Controllers
{
	public class DocumentsController : AbstractController
	{
		#region Properties, fields, constructors

		public DocumentsController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public void GetDocuments(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentModel>(request.Filters),
				rows = _FilteringDao.List<DocumentModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		#endregion
	}
}