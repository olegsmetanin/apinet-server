using System;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.System.Model;
using Newtonsoft.Json;

namespace AGO.System.Controllers
{
	public class UsersController : AbstractController
	{
		#region Properties, fields, constructors

		public UsersController(
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

		#region Constants

		public const string FilterNameProperty = "name";

		public const string FilterGroupProperty = "group";

		public const string FilterProperty = "filter";

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void LoadFilter(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);

			var filterNameProperty = request.Body.Property(FilterNameProperty);
			var filterName = filterNameProperty != null ? filterNameProperty.TokenValue().TrimSafe() : null;
			if (filterName.IsNullOrEmpty())
				throw new MalformedRequestException();

			var filterGroupProperty = request.Body.Property(FilterGroupProperty);
			var filterGroup = filterGroupProperty != null ? filterGroupProperty.TokenValue().TrimSafe() : null;
			if (filterGroup.IsNullOrEmpty())
				throw new MalformedRequestException();

			var currentUser = _AuthController.GetCurrentUser();

			var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.Name == filterName && m.GroupName == filterGroup && m.User == currentUser)
				.Take(1).List().FirstOrDefault();

			output.WriteRaw(filterModel != null ? filterModel.Filter : "null");
		}

		[JsonEndpoint, RequireAuthorization]
		public void SaveFilter(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);
			var validationResult = new ValidationResult();

			try
			{
				var filterNameProperty = request.Body.Property(FilterNameProperty);
				var filterName = filterNameProperty != null ? filterNameProperty.TokenValue().TrimSafe() : null;
				if (filterName.IsNullOrEmpty())
					throw new MalformedRequestException();

				var filterGroupProperty = request.Body.Property(FilterGroupProperty);
				var filterGroup = filterGroupProperty != null ? filterGroupProperty.TokenValue().TrimSafe() : null;
				if (filterGroup.IsNullOrEmpty())
					throw new MalformedRequestException();

				var filterProperty = request.Body.Property(FilterProperty);
				if (filterProperty == null || filterProperty.Value == null)
					throw new MalformedRequestException();

				var currentUser = _AuthController.GetCurrentUser();

				var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
					.Where(m => m.Name == filterName && m.GroupName == filterGroup && m.User == currentUser)
					.Take(1).List().FirstOrDefault();

				filterModel = filterModel ?? new UserFilterModel
				{
					Name = filterName,
					GroupName = filterGroup,
					User = currentUser
				};
				filterModel.Filter = filterProperty.Value.ToString();

				_CrudDao.Store(filterModel);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}
			finally
			{
				_JsonService.CreateSerializer().Serialize(output, new { success = true });
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteFilter(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);
			
			var filterNameProperty = request.Body.Property(FilterNameProperty);
			var filterName = filterNameProperty != null ? filterNameProperty.TokenValue().TrimSafe() : null;
			if (filterName.IsNullOrEmpty())
				throw new MalformedRequestException();

			var filterGroupProperty = request.Body.Property(FilterGroupProperty);
			var filterGroup = filterGroupProperty != null ? filterGroupProperty.TokenValue().TrimSafe() : null;
			if (filterGroup.IsNullOrEmpty())
				throw new MalformedRequestException();

			var currentUser = _AuthController.GetCurrentUser();

			var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.Name == filterName && m.GroupName == filterGroup && m.User == currentUser)
				.Take(1).List().FirstOrDefault();
			if (filterModel != null)
				_CrudDao.Delete(filterModel);

			_JsonService.CreateSerializer().Serialize(output, new { success = true });	
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetFilterNames(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);

			var filterGroupProperty = request.Body.Property(FilterGroupProperty);
			var filterGroup = filterGroupProperty != null ? filterGroupProperty.TokenValue().TrimSafe() : null;
			if (filterGroup.IsNullOrEmpty())
				throw new MalformedRequestException();

			var currentUser = _AuthController.GetCurrentUser();

			var query = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
			    .Where(m => m.GroupName == filterGroup && m.User == currentUser)
				.OrderBy(m => m.Name).Asc
				.Select(m => m.Name);

			_JsonService.CreateSerializer().Serialize(output, new {rows = query.List<string>() });
		}

		#endregion
	}
}