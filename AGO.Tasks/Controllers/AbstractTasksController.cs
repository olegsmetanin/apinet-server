using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using Common.Logging;
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
			AuthController authController,
			ISecurityService securityService) 
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService)
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

			return _CrudDao.PagedQuery(query, page).LookupModelsList(textProperty).ToArray();
		}

		protected UpdateResult<TDTO> Edit<TModel, TDTO>(Guid id, string project, 
			Action<TModel, ValidationResult> update,
			Func<TModel, TDTO> convert, 
			Func<TModel> factory = null) where TDTO: class
			where TModel: CoreModel<Guid>, new()
		{
			var result = new UpdateResult<TDTO> {Validation = new ValidationResult()};
			Func<TModel> defaultFactory = () =>
			{
				var m = new TModel();
				var secureModel = m as ISecureModel;
				if (secureModel != null)
					secureModel.Creator = _AuthController.CurrentUser();
			    var projectBoundModel = m as IProjectBoundModel;
			    if (projectBoundModel != null)
			        projectBoundModel.ProjectCode = project;
			    return m;
			};

			try
			{
				var persistentModel = default(Guid).Equals(id)
				                      	?  (factory ?? defaultFactory)()
				                      	: _CrudDao.Get<TModel>(id);
				if (persistentModel == null)
					throw new NoSuchEntityException();

				update(persistentModel, result.Validation);
				//validate model
				_ModelProcessingService.ValidateModelSaving(persistentModel, result.Validation);
				if (!result.Validation.Success)
					return result;
				//test permissions
				SecurityService.DemandUpdate(persistentModel, project, _AuthController.CurrentUser().Id, Session);
				//persist
				_CrudDao.Store(persistentModel);

				result.Model = convert(persistentModel);
			}
			catch (Exception e)
			{
				LogManager.GetLogger(GetType()).Error(e.GetBaseException().Message, e);
				var msg = _LocalizationService.MessageForException(e) ?? "Unexpected error";
				result.Validation.AddErrors(msg);
			}

			return result;
		}
	}
}