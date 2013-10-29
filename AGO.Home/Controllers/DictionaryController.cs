using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using NHibernate.Criterion;

namespace AGO.Home.Controllers
{
	public enum TagsRequestMode
	{
		Personal,
		Common
	}

	public class DictionaryController : AbstractController
	{
		#region Properties, fields, constructors

		public DictionaryController(
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

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectStatuses(
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>()
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return query.PagedQuery(_CrudDao, page).LookupModelsList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectStatusDescriptions(
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>()
				.Select(Projections.Distinct(Projections.Property("Description")))
				.OrderBy(m => m.Description).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Description).IsLike(term, MatchMode.Anywhere);

			return query.PagedQuery(_CrudDao, page).LookupList(m => m.Description);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectStatusModel> GetProjectStatuses(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			return _FilteringDao.List<ProjectStatusModel>(filter, new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public ProjectStatusModel GetProjectStatus([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<ProjectStatusModel, Guid>(id, dontFetchReferences);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectStatusMetadata()
		{
			return MetadataForModelAndRelations<ProjectStatusModel>();
		}

		[JsonEndpoint, RequireAuthorization(true)]
		public object EditProjectStatus([NotNull] ProjectStatusModel model)
		{
			var validation = new ValidationResult();

			try
			{
				var persistentModel = default(Guid).Equals(model.Id)
					? new ProjectStatusModel { Creator = _AuthController.CurrentUser() }
					: _CrudDao.Get<ProjectStatusModel>(model.Id, true);
				persistentModel.Name = model.Name.TrimSafe();
				persistentModel.Description = model.Description.TrimSafe();

				_ModelProcessingService.ValidateModelSaving(persistentModel, validation);
				if (!validation.Success)
					return validation;
				
				_CrudDao.Store(persistentModel);
				return persistentModel;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
				return validation;
			}			
		}

		[JsonEndpoint, RequireAuthorization(true)]
		public bool DeleteProjectStatuses([NotEmpty] ICollection<Guid> ids, Guid? replaceId)
		{
			ids = ids.Where(id => !default(Guid).Equals(id)).ToList();
			replaceId = replaceId ?? default(Guid);

			if (ids.Any(replaceId.Value.Equals))
				throw new CanNotReplaceWithItemThatWillBeDeletedTo();
			
			var session = _SessionProvider.CurrentSession;
			using (var transaction = _SessionProvider.CurrentSession.BeginTransaction())
			{
				var replace = !default(Guid).Equals(replaceId.Value);
				var updateQuery = replace 
					? session.CreateQuery("update versioned ProjectModel set StatusId = :newId where StatusId = :oldId") 
					: null;
				var historyUpdateQuery = replace
					? session.CreateQuery("update versioned ProjectStatusHistoryModel set StatusId = :newId where StatusId = :oldId")
					: null;

				foreach (var id in ids)
				{
					if (replace)
					{
						updateQuery
							.SetGuid("newId", replaceId.Value)
							.SetGuid("oldId", id)
							.ExecuteUpdate();
						historyUpdateQuery
							.SetGuid("newId", replaceId.Value)
							.SetGuid("oldId", id)
							.ExecuteUpdate();
					}

					var model = _CrudDao.Get<ProjectStatusModel>(id, true);

					if (_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.Status == model)))
						throw new CannotDeleteReferencedItemException();

					if (_CrudDao.Exists<ProjectStatusHistoryModel>(q => q.Where(m => m.Status == model)))
						throw new CannotDeleteReferencedItemException();

					_CrudDao.Delete(model);
				}

				transaction.Commit();
			}

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectTagModel> GetProjectTags(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			TagsRequestMode mode)
		{
			filter.Add(ProjectTagModeFilter(mode));

			return _FilteringDao.List<ProjectTagModel>(filter, new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public ProjectTagModel GetProjectTag([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<ProjectTagModel, Guid>(id, dontFetchReferences);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectTags(
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectTagModel>()
				.Where(m => m.Owner == _AuthController.CurrentUser() || m.Owner == null)
				.OrderBy(m => m.Name).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return query.PagedQuery(_CrudDao, page).LookupModelsList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectTagMetadata()
		{
			return MetadataForModelAndRelations<ProjectTagModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult EditProjectTag([NotNull] ProjectTagModel model, TagsRequestMode mode)
		{
			var validation = new ValidationResult();

			try
			{
				var currentUser = _AuthController.CurrentUser();				
				var persistentModel = default(Guid).Equals(model.Id) 
					? new ProjectTagModel
						{
							Creator = currentUser,
							Owner = mode == TagsRequestMode.Personal ? currentUser : null
						}	
					: _CrudDao.Get<ProjectTagModel>(model.Id, true);

				if (persistentModel.Owner != null && !currentUser.Equals(persistentModel.Owner) && currentUser.SystemRole != SystemRole.Administrator)
					throw new AccessForbiddenException();
				persistentModel.Name = model.Name.TrimSafe();

				if (_SessionProvider.CurrentSession.QueryOver<ProjectTagModel>().Where(
						m => m.Name == persistentModel.Name && m.Owner == persistentModel.Owner && m.Id != persistentModel.Id).RowCount() > 0)
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));

				_ModelProcessingService.ValidateModelSaving(persistentModel, validation);
				if (!validation.Success)
					return validation;

				var parentsStack = new Stack<TagModel>();

				var current = persistentModel as TagModel;
				while (current != null)
				{
					parentsStack.Push(current);
					current = current.Parent;
				}

				var fullName = new StringBuilder();
				while (parentsStack.Count > 0)
				{
					current = parentsStack.Pop();
					if (fullName.Length > 0)
						fullName.Append(" / ");
					fullName.Append(current.Name);
				}
				persistentModel.FullName = fullName.ToString();

				_CrudDao.Store(persistentModel);
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}
			
			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteProjectTag([NotEmpty] Guid id)
		{
			var model = _CrudDao.Get<ProjectTagModel>(id, true);

			var currentUser = _AuthController.CurrentUser();
			if (model.Owner != null && !currentUser.Equals(model.Owner) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			if (_SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
					.Where(m => m.Tag == model).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(model);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectTypes(
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectTypeModel>()
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return query.PagedQuery(_CrudDao, page).LookupModelsList(m => m.Name);
		}

		#endregion

		#region Helper methods

		protected IModelFilterNode ProjectTagModeFilter(TagsRequestMode mode)
		{
			var result = new ModelFilterNode();
			if (mode == TagsRequestMode.Personal)
			{
				result.AddItem(new ValueFilterNode
				{
					Path = "Owner",
					Operator = ValueFilterOperators.Eq,
					Operand = _AuthController.CurrentUser().Id.ToString()
				});
			}
			else
			{
				result.AddItem(new ValueFilterNode
				{
					Path = "Owner",
					Operator = ValueFilterOperators.Exists,
					Negative = true
				});
			}
			return result;
		}

		#endregion
	}
}