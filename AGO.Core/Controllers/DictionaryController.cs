using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using NHibernate.Criterion;

namespace AGO.Core.Controllers
{
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
			Guid? parentId,
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			var ownerFilter = new ModelFilterNode();
			ownerFilter.AddItem(new ValueFilterNode
			{
				Path = "Creator",
				Operator = ValueFilterOperators.Eq,
				Operand = _AuthController.CurrentUser().Id.ToString()
			});

			filter.Add(ownerFilter);

			var parentFilter = new ModelFilterNode();
			if (parentId != null && !default(Guid).Equals(parentId))
			{
				parentFilter.AddItem(new ValueFilterNode
				{
					Path = "Parent",
					Operator = ValueFilterOperators.Eq,
					Operand = parentId.ToStringSafe()
				});
			}
			else
			{
				parentFilter.AddItem(new ValueFilterNode
				{
					Path = "Parent",
					Operator = ValueFilterOperators.Exists,
					Negative = true
				});
			}
			filter.Add(parentFilter);

			return _FilteringDao.List<ProjectTagModel>(filter, new FilteringOptions
			{
				Page = page,
				Sorters = sorters
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetProjectTagsCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			var ownerFilter = new ModelFilterNode();
			ownerFilter.AddItem(new ValueFilterNode
			{
				Path = "Creator",
				Operator = ValueFilterOperators.Eq,
				Operand = _AuthController.CurrentUser().Id.ToString()
			});

			filter.Add(ownerFilter);

			return _FilteringDao.RowCount<ProjectTagModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectTags(
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<ProjectTagModel>()
				.Where(m => m.Creator == _AuthController.CurrentUser())
				.OrderBy(m => m.FullName).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term, MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.FullName);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectTagMetadata()
		{
			return MetadataForModelAndRelations<ProjectTagModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public object CreateProjectTag(Guid parentId, [NotEmpty] string name)
		{
			var validation = new ValidationResult();

			try
			{
				var currentUser = _AuthController.CurrentUser();

				var tag = new ProjectTagModel
				{
					Creator = currentUser,
					Name = name.TrimSafe(),
					Parent = !default(Guid).Equals(parentId) ? _CrudDao.Get<ProjectTagModel>(parentId, true) : null
				};

				if (tag.Parent != null && !tag.Creator.Equals(tag.Parent.Creator) && currentUser.SystemRole != SystemRole.Administrator)
					throw new AccessForbiddenException();

				if (_SessionProvider.CurrentSession.QueryOver<ProjectTagModel>().Where(
						m => m.Name == tag.Name && m.Parent == tag.Parent && m.Creator == tag.Creator).RowCount() > 0)
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));

				_ModelProcessingService.ValidateModelSaving(tag, validation);
				if (!validation.Success)
					return validation;

				var parentsStack = new Stack<TagModel>();

				var current = tag as TagModel;
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
				tag.FullName = fullName.ToString();

				_CrudDao.Store(tag);
				return tag;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public object UpdateProjectTag([NotEmpty] Guid id, [NotEmpty] string name)
		{
			var validation = new ValidationResult();

			try
			{
				var currentUser = _AuthController.CurrentUser();
				var tag = _CrudDao.Get<ProjectTagModel>(id, true);

				if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
					throw new AccessForbiddenException();

				tag.Name = name.TrimSafe();

				if (_SessionProvider.CurrentSession.QueryOver<ProjectTagModel>().Where(
						m => m.Name == tag.Name && m.Parent == tag.Parent && m.Creator == tag.Creator && m.Id != tag.Id).RowCount() > 0)
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));

				_ModelProcessingService.ValidateModelSaving(tag, validation);
				if (!validation.Success)
					return validation;

				var parentsStack = new Stack<TagModel>();

				var current = tag as TagModel;
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
				tag.FullName = fullName.ToString();

				_CrudDao.Store(tag);
				return tag;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult DeleteProjectTag([NotEmpty] Guid id)
		{
			var validation = new ValidationResult();

			try
			{
				DoDeleteProjectTag(_CrudDao.Get<ProjectTagModel>(id, true), _AuthController.CurrentUser());
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
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

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupCustomPropertyTypes(
			[NotEmpty] string project,
			[InRange(0, null)] int page,
			string term)
		{
			var query = _SessionProvider.CurrentSession.QueryOver<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.Name).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return _CrudDao.PagedQuery(query, page).LookupModelsList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public CustomPropertyTypeModel GetCustomPropertyType([NotEmpty] Guid id, bool dontFetchReferences)
		{
			return GetModel<CustomPropertyTypeModel, Guid>(id, dontFetchReferences);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> CustomPropertyMetadata()
		{
			return MetadataForModelAndRelations<CustomPropertyTypeModel>().Concat(
				MetadataForModelAndRelations<CustomPropertyInstanceModel>());
		}

		#endregion

		#region Helper methods

		public void DoDeleteProjectTag(ProjectTagModel tag, UserModel currentUser)
		{
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			if (_SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
					.Where(m => m.Tag == tag).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			foreach (var subTag in _SessionProvider.CurrentSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Parent == tag).List())
				DoDeleteProjectTag(subTag, currentUser);

			_CrudDao.Delete(tag);
		}

		#endregion
	}
}