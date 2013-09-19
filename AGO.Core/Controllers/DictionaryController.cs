using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters.Metadata;
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
		public object GetCustomPropertyTypes(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			return new
			{
				totalRowsCount = _FilteringDao.RowCount<CustomPropertyTypeModel>(request.Filters),
				rows = _FilteringDao.List<CustomPropertyTypeModel>(request.Filters, OptionsFromRequest(request))
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public CustomPropertyTypeModel GetCustomPropertyType(JsonReader input)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode {Operator = ModelFilterOperators.And};
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id", 
				Operator = ValueFilterOperators.Eq, 
				Operand = request.Id.ToStringSafe()
			});

			return _FilteringDao.List<CustomPropertyTypeModel>(
				new[] {filter}, OptionsFromRequest(request)).FirstOrDefault();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> CustomPropertyMetadata(JsonReader input)
		{
			return MetadataForModelAndRelations<CustomPropertyTypeModel>();
		}

		#endregion
	}
}