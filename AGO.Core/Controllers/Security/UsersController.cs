using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
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
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory,
			IStateStorage<string> clientStateStorage)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory)
		{
			if (clientStateStorage == null)
				throw new ArgumentNullException("clientStateStorage");
			_ClientStateStorage = clientStateStorage;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public JToken LoadFilter(
			[NotEmpty] string project,
			[NotEmpty] string name,
			[NotEmpty] string group)
		{
			var s = project.IsNullOrWhiteSpace() ? MainSession : ProjectSession(project);

			var filterModel = s.QueryOver<UserFilterModel>()
				.Where(m => m.Name == name && m.GroupName == group && m.OwnerId == CurrentUser.Id)
				.Take(1).List().FirstOrDefault();

			return filterModel != null ? JToken.Parse(filterModel.Filter) : null;
		}

		//TODO refactoring: where stored userfilter???
		[JsonEndpoint, RequireAuthorization]
		public ValidationResult SaveFilter(
			[NotEmpty] string project,
			[NotEmpty] string name,
			[NotEmpty] string group,
			[NotNull] JToken filter)
		{
			var validation = new ValidationResult();
			var s = project.IsNullOrWhiteSpace() ? MainSession : ProjectSession(project);
			var dao = project.IsNullOrWhiteSpace() ? DaoFactory.CreateMainCrudDao() : DaoFactory.CreateProjectCrudDao(project);

			try
			{
				//TODO: Валидации длины

				var persistentModel = s.QueryOver<UserFilterModel>()
					.Where(m => m.Name == name && m.GroupName == group && m.OwnerId == CurrentUser.Id)
					.Take(1).List().FirstOrDefault() ?? new UserFilterModel
					{
						Name = name,
						GroupName = group,
						OwnerId = CurrentUser.Id
					};
				persistentModel.Filter = filter.ToString();

				_ModelProcessingService.ValidateModelSaving(persistentModel, validation, s);
				if (!validation.Success)
					return validation;

				dao.Store(persistentModel);
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteFilter(
			[NotEmpty] string project,
			[NotEmpty] string name,
			[NotEmpty] string group)
		{
			var s = project.IsNullOrWhiteSpace() ? MainSession : ProjectSession(project);
			var dao = project.IsNullOrWhiteSpace() ? DaoFactory.CreateMainCrudDao() : DaoFactory.CreateProjectCrudDao(project);

			var filterModel = s.QueryOver<UserFilterModel>()
				.Where(m => m.Name == name && m.GroupName == group && m.OwnerId == CurrentUser.Id)
				.Take(1).List().FirstOrDefault();
			if (filterModel != null)
				dao.Delete(filterModel);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupFilterNames(
			[NotEmpty] string project,
			[NotEmpty] string group,
			[InRange(0, null)] int page,
			string term)
		{
			var s = project.IsNullOrWhiteSpace() ? MainSession : ProjectSession(project);
			var dao = project.IsNullOrWhiteSpace() ? DaoFactory.CreateMainCrudDao() : DaoFactory.CreateProjectCrudDao(project);

			var query = s.QueryOver<UserFilterModel>()
				.Where(m => m.GroupName == group && m.OwnerId == CurrentUser.Id)
				.OrderBy(m => m.Name).Asc
				.Select(m => m.Name);
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return dao.PagedQuery(query, page).LookupList(m => m.Name, m => m.Name, false);
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

		[JsonEndpoint]
		public string GetLocale()
		{
			return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupUsers(string term, [InRange(0, null)] int page)
		{
			var query = MainSession.QueryOver<UserModel>()
				.Where(m => m.Active)
				.OrderBy(m => m.FullName).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term, MatchMode.Anywhere);

			return DaoFactory.CreateMainCrudDao().PagedQuery(query, page).LookupModelsList(m => m.FullName);
		}

		#endregion
	}
}