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

		protected virtual UserModel CurrentUser
		{
			get { return _AuthController.CurrentUser(); }
		}

		protected IEnumerable<LookupEntry> Lookup<TModel>(string project, string term, int page,
			Expression<Func<TModel, object>> textProperty,
			Expression<Func<TModel, object>> searchProperty = null,
			params Expression<Func<TModel, object>>[] sorters)
			where TModel: class, IProjectBoundModel, IIdentifiedModel<Guid>
		{
			try
			{
				//project predicate
				var projectFilter = _FilteringService.Filter<TModel>().Where(m => m.ProjectCode == project);
				//term search predicate
				IModelFilterNode termFilter = null;
				if (!term.IsNullOrWhiteSpace())
				{
					var search = (searchProperty ?? textProperty).Cast<TModel, object, string>();
					termFilter = _FilteringService.Filter<TModel>().WhereString(search).Like(term.TrimSafe(), true, true);
				}
				//concat with security predicates
				var filter = SecurityService.ApplyReadConstraint<TModel>(project, CurrentUser.Id, Session, projectFilter, termFilter);
				//get executable criteria
				var criteria = _FilteringService.CompileFilter(filter, typeof(TModel)).GetExecutableCriteria(Session);
				//add needed sorting
				if (sorters == null || !sorters.Any())
				{
					criteria.AddOrder(Order.Asc(Projections.Property(textProperty).PropertyName));
				}
				else
				{
					foreach (var s in sorters)
					{
						criteria.AddOrder(Order.Asc(Projections.Property(s).PropertyName));
					}
				}

				return _CrudDao.PagedCriteria(criteria, page).LookupModelsList(textProperty);
			}
			catch (NoSuchProjectMemberException)
			{
				Log.WarnFormat("Lookup from not project member catched. User '{0}' for type '{1}'", CurrentUser.Login, typeof(TModel).AssemblyQualifiedName);
				return Enumerable.Empty<LookupEntry>();
			}
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