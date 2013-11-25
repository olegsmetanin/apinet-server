using System;
using System.Collections.Generic;
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

		private static IDictionary<string, LookupEntry[]> projectStatuses;

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectStatuses(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<ProjectStatus>(term, page, ref projectStatuses);
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
		public int GetProjectTagsCount([NotNull] ICollection<IModelFilterNode> filter, TagsRequestMode mode)
		{
			filter.Add(ProjectTagModeFilter(mode));

			return _FilteringDao.RowCount<ProjectTagModel>(filter);
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

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.Name);
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

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.Name);
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