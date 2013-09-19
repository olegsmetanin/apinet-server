using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
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
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public object GetDocuments(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentModel>(filter),
				rows = _FilteringDao.List<DocumentModel>(filter, new FilteringOptions
				{
					Skip = page * pageSize,
					Take = pageSize,
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