using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Modules.Attributes;
using AGO.Core.Validation;
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
			ILocalizationService localizationService,
			IValidationService validationService,
			AuthController authController)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, validationService, authController)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public JToken LoadFilter(
			[NotEmpty] string name, 
			[NotEmpty] string group)
		{
			var currentUser = _AuthController.CurrentUser();

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
			var validation = new ValidationResult();

			try
			{
				//TODO: Валидации длины
				var currentUser = _AuthController.CurrentUser();

				var persistentModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
					.Where(m => m.Name == name && m.GroupName == group && m.User == currentUser)
					.Take(1).List().FirstOrDefault() ?? new UserFilterModel
				{
					Name = name,
					GroupName = group,
					User = currentUser
				};
				persistentModel.Filter = filter.ToString();

				_ValidationService.ValidateModel(persistentModel, validation);			
				if (!validation.Success)
					return validation;

				_CrudDao.Store(persistentModel);
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}
			
			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteFilter(
			[NotEmpty] string name,
			[NotEmpty] string group)
		{
			var currentUser = _AuthController.CurrentUser();

			var filterModel = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.Name == name && m.GroupName == group && m.User == currentUser)
				.Take(1).List().FirstOrDefault();
			if (filterModel != null)
				_CrudDao.Delete(filterModel);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<string> GetFilterNames([NotEmpty] string group)
		{
			return _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
			    .Where(m => m.GroupName == group && m.User == _AuthController.CurrentUser())
				.OrderBy(m => m.Name).Asc
				.Select(m => m.Name)
				.List<string>();
		}

		#endregion
	}
}