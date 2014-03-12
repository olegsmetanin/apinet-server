using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Security;
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
using AGO.Core.Security;
using NHibernate;
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
			AuthController authController,
			ISecurityService securityService)
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService)
		{
		}

		#endregion

		#region Json endpoints

		private static IDictionary<string, LookupEntry[]> projectStatuses;

		private UserModel CurrentUser
		{
			get { return _AuthController.CurrentUser(); }
		}

		private ISession Session
		{
			get { return _SessionProvider.CurrentSession; }
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectStatuses(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<ProjectStatus>(term, page, ref projectStatuses);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectTags(
			[InRange(0, null)] int page,
			string term)
		{
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
				termFilter = _FilteringService.Filter<ProjectTagModel>()
					.WhereString(m => m.FullName).Like(term, true, true);

			var filter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null, 
				CurrentUser.Id, Session, termFilter);

			var criteria = _FilteringService.CompileFilter(filter, typeof (ProjectTagModel))
				.GetExecutableCriteria(_SessionProvider.CurrentSession)
				.AddOrder(Order.Asc(Projections.Property<ProjectTagModel>(m => m.FullName)));
			return _CrudDao
				.PagedCriteria(criteria, page)
				.LookupModelsList<ProjectTagModel>(m => m.FullName);
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
		public IEnumerable<ProjectTagModel> GetProjectTags(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			var finalFilter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null,
				CurrentUser.Id, Session, filter.ToArray());

			return _FilteringDao.List<ProjectTagModel>(finalFilter, page, sorters);
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetProjectTagsCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			var finalFilter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null,
				CurrentUser.Id, Session, filter.ToArray());

			return _FilteringDao.RowCount<ProjectTagModel>(finalFilter);
		}

		[JsonEndpoint, RequireAuthorization]
		public object CreateProjectTag(Guid parentId, [NotEmpty] string name)
		{
			var validation = new ValidationResult();

			try
			{
				var tag = new ProjectTagModel
				{
					Creator = CurrentUser,
					Name = name.TrimSafe(),
					Parent = !default(Guid).Equals(parentId) ? _CrudDao.Get<ProjectTagModel>(parentId, true) : null
				};

				SecurityService.DemandUpdate(tag, null, CurrentUser.Id, Session);

				if (_CrudDao.Exists<ProjectTagModel>(q => q.Where(m => 
						m.Name == tag.Name 
						&& m.Parent == tag.Parent 
						&& m.Creator == tag.Creator)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<ProjectTagModel>();
				DoUpdateProjectTag(tag, affected);
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
				var tag = _CrudDao.Get<ProjectTagModel>(id, true);

				SecurityService.DemandUpdate(tag, null, CurrentUser.Id, Session);

				tag.Name = name.TrimSafe();

				if (_CrudDao.Exists<ProjectTagModel>(q => q.Where(m => 
						m.Name == tag.Name 
						&& m.Parent == tag.Parent 
						&& m.Creator == tag.Creator 
						&& m.Id != tag.Id)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<ProjectTagModel>();
				DoUpdateProjectTag(tag, affected);
				return affected;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public object DeleteProjectTag([NotEmpty] Guid id)
		{
			var validation = new ValidationResult();

			try
			{
				var tag = _CrudDao.Get<ProjectTagModel>(id, true);
				SecurityService.DemandDelete(tag, null, CurrentUser.Id, Session);
				var deletedIds = new HashSet<Guid>();
				DoDeleteProjectTag(tag, CurrentUser, deletedIds);
				return deletedIds;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
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

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ProjectTagMetadata()
		{
			return MetadataForModelAndRelations<ProjectTagModel>();
		}

		#endregion

		#region Helper methods

		protected void DoDeleteProjectTag(ProjectTagModel tag, UserModel currentUser, ISet<Guid> deletedIds)
		{
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			foreach (var subTag in _SessionProvider.CurrentSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Parent == tag).List())
				DoDeleteProjectTag(subTag, currentUser, deletedIds);

			deletedIds.Add(tag.Id);
			_CrudDao.Delete(tag);
		}

		protected void DoUpdateProjectTag(ProjectTagModel tag, ISet<ProjectTagModel> affected)
		{
			if (tag == null)
				return;

			var fullName = new StringBuilder(tag.Parent != null ? tag.Parent.FullName : string.Empty);		
			if (fullName.Length > 0)
				fullName.Append(" / ");
			fullName.Append(tag.Name);

			tag.FullName = fullName.ToString();

			_CrudDao.Store(tag);
			affected.Add(tag);

			foreach (var subTag in tag.Children.OfType<ProjectTagModel>())
				DoUpdateProjectTag(subTag, affected);
		}

		#endregion
	}
}