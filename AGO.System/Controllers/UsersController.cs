using System;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Modules.Attributes;
using AGO.System.Model;
using Newtonsoft.Json.Linq;

namespace AGO.System.Controllers
{
	public class UsersController : AbstractController
	{
		#region Properties, fields, constructors

		public UsersController(
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
		public JToken LoadFilter(
			[NotEmpty] string name, 
			[NotEmpty] string group)
		{
			var currentUser = _AuthController.GetCurrentUser();

			var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.Name == name && m.GroupName == group && m.User == currentUser)
				.Take(1).List().FirstOrDefault();

			return filterModel != null ? JToken.Parse(filterModel.Filter) : null;
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult SaveFilter(
			[NotEmpty] string name, 
			[NotEmpty] string group, 
			[NotNull] JToken filter)
		{
			var validationResult = new ValidationResult();

			try
			{
				//TODO: Валидации длины
				var currentUser = _AuthController.GetCurrentUser();

				var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
					.Where(m => m.Name == name && m.GroupName == group && m.User == currentUser)
					.Take(1).List().FirstOrDefault();

				filterModel = filterModel ?? new UserFilterModel
				{
					Name = name,
					GroupName = group,
					User = currentUser
				};
				filterModel.Filter = filter.ToString();

				_CrudDao.Store(filterModel);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}
			
			return validationResult;
		}

		[JsonEndpoint, RequireAuthorization]
		public object DeleteFilter(
			[NotEmpty] string name,
			[NotEmpty] string group)
		{
			var currentUser = _AuthController.GetCurrentUser();

			var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.Name == name && m.GroupName == group && m.User == currentUser)
				.Take(1).List().FirstOrDefault();
			if (filterModel != null)
				_CrudDao.Delete(filterModel);

			return new { success = true };	
		}

		[JsonEndpoint, RequireAuthorization]
		public object GetFilterNames([NotEmpty] string group)
		{
			var currentUser = _AuthController.GetCurrentUser();

			var query = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
			    .Where(m => m.GroupName == group && m.User == currentUser)
				.OrderBy(m => m.Name).Asc
				.Select(m => m.Name);

			return new {rows = query.List<string>() };
		}

		#endregion
	}
}