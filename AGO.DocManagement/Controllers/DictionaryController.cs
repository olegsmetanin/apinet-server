using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.DocManagement.Model.Dictionary.Documents;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;

namespace AGO.DocManagement.Controllers
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
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public object GetDocumentStatuses(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentStatusModel>(filter),
				rows = _FilteringDao.List<DocumentStatusModel>(filter, new FilteringOptions
				{
					Skip = page * pageSize,
					Take = pageSize,
					Sorters = sorters
				})
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public DocumentStatusModel GetDocumentStatus([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<DocumentStatusModel, Guid>(id, dontFetchReferences);
		}

		[JsonEndpoint, RequireAuthorization]
		public object GetDocumentCategories(
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentCategoryModel>(filter),
				rows = _FilteringDao.List<DocumentCategoryModel>(filter, new FilteringOptions
				{
					Skip = page * pageSize,
					Take = pageSize,
					Sorters = sorters
				})
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public DocumentCategoryModel GetDocumentCategory([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<DocumentCategoryModel, Guid>(id, dontFetchReferences);
		}

		#endregion
	}
}