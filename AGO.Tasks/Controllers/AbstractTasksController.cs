using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
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
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory) 
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService, registry, factory)
		{
		}

		protected ICriteria PrepareLookup<TModel>(string project, string term, int page,
			Expression<Func<TModel, string>> textProperty,
			Expression<Func<TModel, string>> searchProperty = null,
			params Expression<Func<TModel, object>>[] sorters)
			where TModel : class, IProjectBoundModel, IIdentifiedModel<Guid>
		{
			var projectFilter = _FilteringService.Filter<TModel>().Where(m => m.ProjectCode == project);
			//term search predicate
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
			{
				termFilter = _FilteringService.Filter<TModel>()
					.WhereString(searchProperty ?? textProperty).Like(term.TrimSafe(), true, true);
			}
			//concat with security predicates
			var filter = ApplyReadConstraint<TModel>(project, projectFilter, termFilter);
			//get executable criteria
			var criteria = _FilteringService.CompileFilter(filter, typeof(TModel)).GetExecutableCriteria(ProjectSession(project));
			//add needed sorting
			var objTextProp = textProperty.Cast<TModel, string, object>();
			if (sorters == null || !sorters.Any())
			{
				criteria.AddOrder(Order.Asc(Projections.Property(objTextProp).PropertyName));
			}
			else
			{
				foreach (var s in sorters)
				{
					criteria.AddOrder(Order.Asc(Projections.Property(s).PropertyName));
				}
			}

			return DaoFactory.CreateProjectCrudDao(project).PagedCriteria(criteria, page);
		}

		protected IEnumerable<LookupEntry> Lookup<TModel>(string project, string term, int page,
			Expression<Func<TModel, string>> textProperty,
			Expression<Func<TModel, string>> searchProperty = null,
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
					termFilter = _FilteringService.Filter<TModel>()
						.WhereString(searchProperty ?? textProperty).Like(term.TrimSafe(), true, true);
				}
				//concat with security predicates
				var filter = ApplyReadConstraint<TModel>(project, projectFilter, termFilter);
				//get executable criteria
				var criteria = _FilteringService.CompileFilter(filter, typeof(TModel)).GetExecutableCriteria(ProjectSession(project));
				//add needed sorting
				var objTextProp = textProperty.Cast<TModel, string, object>();
				if (sorters == null || !sorters.Any())
				{
					criteria.AddOrder(Order.Asc(Projections.Property(objTextProp).PropertyName));
				}
				else
				{
					foreach (var s in sorters)
					{
						criteria.AddOrder(Order.Asc(Projections.Property(s).PropertyName));
					}
				}

				return PrepareLookup(project, term, page, textProperty, searchProperty, sorters)
					.LookupModelsList(objTextProp);
			}
			catch (NoSuchProjectMemberException)
			{
				Log.WarnFormat("Lookup from not project member catched. User '{0}' for type '{1}'", CurrentUser.Email, typeof(TModel).AssemblyQualifiedName);
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
				{
					var member = CurrentUserToMember(project);
					secureModel.Creator = member;
					secureModel.LastChanger = member;
					secureModel.LastChangeTime = DateTime.UtcNow;
				}
				var projectBoundModel = m as IProjectBoundModel;
			    if (projectBoundModel != null)
			        projectBoundModel.ProjectCode = project;
			    return m;
			};

			Func<TModel, TModel> postFactory = model =>
			{
				var secureModel = model as ISecureModel;
				if (secureModel != null)
				{
					secureModel.LastChanger = CurrentUserToMember(project);
					secureModel.LastChangeTime = DateTime.UtcNow;
				}

				return model;
			};

			try
			{
				var dao = DaoFactory.CreateProjectCrudDao(project);

				var persistentModel = default(Guid).Equals(id)
										? postFactory((factory ?? defaultFactory)())
				                      	: dao.Get<TModel>(id);
				if (persistentModel == null)
					throw new NoSuchEntityException();

				TModel original = null;
				if (!persistentModel.IsNew())
					original = (TModel) persistentModel.Clone();

				update(persistentModel, result.Validation);
				//validate model
				_ModelProcessingService.ValidateModelSaving(persistentModel, result.Validation, ProjectSession(project));
				if (!result.Validation.Success)
					return result;
				//test permissions
				DemandUpdate(persistentModel, project);
				//persist
				dao.Store(persistentModel);

				if (original != null)
					_ModelProcessingService.AfterModelUpdated(persistentModel, original);
				else
					_ModelProcessingService.AfterModelCreated(persistentModel);

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

		protected IModelFilterNode ApplyReadConstraint<TModel>(string project, params IModelFilterNode[] filter)
		{
			return SecurityService.ApplyReadConstraint<TModel>(project, CurrentUser.Id, ProjectSession(project), filter);
		}

		protected void DemandUpdate(IdentifiedModel model, string project)
		{
			SecurityService.DemandUpdate(model, project, CurrentUser.Id, ProjectSession(project));
		}

		protected void DemandDelete(IdentifiedModel model, string project)
		{
			SecurityService.DemandDelete(model, project, CurrentUser.Id, ProjectSession(project));
		}

		protected TModel SecureFind<TModel>(string project, Guid id) where TModel : class, IProjectBoundModel, IIdentifiedModel<Guid>
		{
			var fb = _FilteringService.Filter<TModel>();
			var predicate = ApplyReadConstraint<TModel>(project, fb.Where(m => 
				m.ProjectCode == project && m.Id == id));
			var model = DaoFactory.CreateProjectFilteringDao(project).Find<TModel>(predicate);
			if (model == null)
				throw new NoSuchEntityException();

			return model;
		}
	}
}