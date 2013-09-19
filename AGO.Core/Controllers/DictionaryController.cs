using System;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Json;
using AGO.Core.Model.Dictionary;
using AGO.Core.Filters;
using AGO.Core.Modules.Attributes;
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
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void GetCustomPropertyTypes(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<CustomPropertyTypeModel>(request.Filters),
				rows = _FilteringDao.List<CustomPropertyTypeModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
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

		[JsonEndpoint, RequireAuthorization]
		public void CustomPropertyMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<CustomPropertyTypeModel>());
		}

		#endregion
	}
}