using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using NHibernate;
using NHibernate.Criterion;

namespace AGO.Tasks.Controllers
{
	public class AbstractTasksController : AbstractController
	{
		protected AbstractTasksController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ICrudDao crudDao, 
			IFilteringDao filteringDao, 
			ISessionProvider sessionProvider, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController) 
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		protected virtual ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}

		protected IEnumerable<LookupEntry> Lookup<TModel>(string project, string term, int page,
			Expression<Func<TModel, object>> textProperty,
			Expression<Func<TModel, object>> searchProperty = null,
			params Expression<Func<TModel, object>>[] sorters)
			where TModel: class, IProjectBoundModel, IIdentifiedModel<Guid>
		{
			var query = _SessionProvider.CurrentSession.QueryOver<TModel>()
				.Where(m => m.ProjectCode == project);
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(searchProperty ?? textProperty).IsLike(term, MatchMode.Anywhere);

			if (sorters == null || !sorters.Any())
			{
				query = query.OrderBy(textProperty).Asc;
			}
			else
			{
				query = query.OrderBy(sorters[0]).Asc;
				for (var i = 1; i < sorters.Length; i++)
				{
					query = query.ThenBy(sorters[i]).Asc;
				}
			}

			return query.PagedQuery(_CrudDao, page).LookupModelsList(textProperty).ToArray();
		}

		protected ValidationResult Edit<TModel>(Guid id, string project, 
		                                        Action<TModel, ValidationResult> update,
		                                        Func<TModel> factory = null) 
			where TModel: SecureProjectBoundModel<Guid>, new()
		{
			var validation = new ValidationResult();
			Func<TModel> defaultFactory = () => new TModel {ProjectCode = project, Creator = _AuthController.CurrentUser()};

			try
			{
				var persistentModel = default(Guid).Equals(id)
				                      	?  (factory ?? defaultFactory)()
				                      	: _CrudDao.Get<TModel>(id, true);

				update(persistentModel, validation);

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
	}
}