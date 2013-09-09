using System;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Home.Controllers
{
	public enum TagsRequestMode
	{
		Personal,
		Common
	}

	public class DictionaryController : AbstractController
	{
		#region Constants

		public const string TagsRequestModeName = "mode";

		#endregion

		#region Properties, fields, constructors

		protected readonly AuthController _AuthController;

		public DictionaryController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;
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
		public void ProjectStatusMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<ProjectStatusModel>());
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTags(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var modeProperty = request.Body.Property(TagsRequestModeName);
			var mode = (modeProperty != null ? modeProperty.TokenValue() : string.Empty)
				.ParseEnumSafe(TagsRequestMode.Personal);

			IModelFilterNode modeFilter = new ModelFilterNode();
			if (mode == TagsRequestMode.Personal)
			{
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "Owner", 
					Operator = ValueFilterOperators.Eq, 
					Operand = _AuthController.GetCurrentUser().Id.ToString()
				});
			}
			else
			{
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "Owner",
					Operator = ValueFilterOperators.Exists,
					Negative = true
				});
			}
			request.Filters.Add(modeFilter);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectTagModel>(request.Filters),
				rows = _FilteringDao.List<ProjectTagModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTag(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectTagModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint, RequireAuthorization]
		public void ProjectTagMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<ProjectTagModel>());
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