using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;

namespace AGO.Core.Controllers.Security
{
	public class UsersController : AbstractController
	{
		#region Constants

		internal const string CurrentCultureKey = "currentLocale";

		#endregion

		#region Properties, fields, constructors

		protected readonly IStateStorage<string> _ClientStateStorage;

		public UsersController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			IStateStorage<string> clientStateStorage)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
			if (clientStateStorage == null)
				throw new ArgumentNullException("clientStateStorage");
			_ClientStateStorage = clientStateStorage;
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

				_ModelProcessingService.ValidateModelSaving(persistentModel, validation);
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
		public IEnumerable<LookupEntry> LookupFilterNames(
			[NotEmpty] string group,
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<UserFilterModel>()
				.Where(m => m.GroupName == group && m.User == _AuthController.CurrentUser())
				.OrderBy(m => m.Name).Asc
				.Select(m => m.Name);
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page).LookupList(m => m.Name, m => m.Name, false);
		}

		[JsonEndpoint, RequireAuthorization]
		public object SetLocale(CultureInfo locale, IEnumerable<string> userLanguages)
		{
			locale = locale ?? _ClientStateStorage[CurrentCultureKey].ConvertSafe<CultureInfo>();
			locale = locale ?? (userLanguages ?? Enumerable.Empty<string>())
				.Select(s => s.Split(';')[0]).FirstOrDefault().ConvertSafe<CultureInfo>();

			if (locale != null && !locale.Equals(CultureInfo.CurrentUICulture) &&
					_LocalizationService.Cultures.Any(c => c.Equals(locale)))
				Thread.CurrentThread.CurrentUICulture = locale;

			var result = CultureInfo.CurrentUICulture.Name;
			_ClientStateStorage[CurrentCultureKey] = result;

			return new { currentLocale = result };
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupUsers(string term, [InRange(0, null)] int page)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Active)
				.OrderBy(m => m.FullName).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term, MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.FullName);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public string GetRole()
		{
			return string.Empty;
		}

		#endregion
	}
}