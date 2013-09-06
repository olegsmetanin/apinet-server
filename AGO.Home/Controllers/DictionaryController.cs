using System;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.Home.Model.Dictionary.Projects;
using Newtonsoft.Json;

namespace AGO.Home.Controllers
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
			ISessionProvider sessionProvider)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectStatuses(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectStatusModel>(request.Filters),
				rows = _FilteringDao.List<ProjectStatusModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectStatus(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectStatusModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTypes(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectTypeModel>(request.Filters),
				rows = _FilteringDao.List<ProjectTypeModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectType(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectTypeModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		#endregion
	}
}