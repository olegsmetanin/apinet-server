using System;
using System.Linq;
using AGO.Core.Json;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Documents;
using AGO.Hibernate;
using AGO.Hibernate.Filters;
using AGO.Hibernate.Json;
using AGO.Hibernate.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	public class DictionaryController : AbstractController
	{
		#region Properties, fields, constructors

		public DictionaryController(
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
		public void GetCustomPropertyTypes(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<CustomPropertyTypeModel>(request.Filters),
				rows = _FilteringDao.List<CustomPropertyTypeModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint]
		public void GetCustomPropertyType(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode {Operator = ModelFilterOperators.And};
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id", 
				Operator = ValueFilterOperators.Eq, 
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<CustomPropertyTypeModel>(
				new[] {filter}, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint]
		public void GetDocumentStatuses(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentStatusModel>(request.Filters),
				rows = _FilteringDao.List<DocumentStatusModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint]
		public void GetDocumentStatus(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<DocumentStatusModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint]
		public void GetDocumentCategories(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<DocumentCategoryModel>(request.Filters),
				rows = _FilteringDao.List<DocumentCategoryModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint]
		public void GetDocumentCategory(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<DocumentCategoryModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		#endregion
	}
}