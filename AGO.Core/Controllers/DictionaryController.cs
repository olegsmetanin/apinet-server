using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
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
using NHibernate.Criterion;

namespace AGO.Core.Controllers
{
	public class DictionaryController : AbstractController
	{
		#region Properties, fields, constructors

		public DictionaryController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory)
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory)
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
		public IEnumerable<LookupEntry> LookupProjectTags(
			[InRange(0, null)] int page,
			string term)
		{
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
				termFilter = _FilteringService.Filter<ProjectTagModel>()
					.WhereString(m => m.FullName).Like(term, true, true);

			var filter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null, 
				CurrentUser.Id, MainSession, termFilter);

			var criteria = _FilteringService.CompileFilter(filter, typeof (ProjectTagModel))
				.GetExecutableCriteria(MainSession)
				.AddOrder(Order.Asc(Projections.Property<ProjectTagModel>(m => m.FullName)));
			return DaoFactory.CreateMainCrudDao()
				.PagedCriteria(criteria, page)
				.LookupModelsList<ProjectTagModel>(m => m.FullName);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectTypes(
			[InRange(0, null)] int page,
			string term)
		{
			var query = MainSession.QueryOver<ProjectTypeModel>()
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return DaoFactory.CreateMainCrudDao().PagedQuery(query, page).LookupModelsList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupCustomPropertyTypes(
			[NotEmpty] string project,
			[InRange(0, null)] int page,
			string term)
		{
			var query = ProjectSession(project).QueryOver<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.Name).Asc;

			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return DaoFactory.CreateProjectCrudDao(project).PagedQuery(query, page).LookupModelsList(m => m.Name);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ProjectTagModel> GetProjectTags(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			var finalFilter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null,
				CurrentUser.Id, MainSession, filter.ToArray());

			return DaoFactory.CreateMainFilteringDao().List<ProjectTagModel>(finalFilter, page, sorters);
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetProjectTagsCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			var finalFilter = SecurityService.ApplyReadConstraint<ProjectTagModel>(null,
				CurrentUser.Id, MainSession, filter.ToArray());

			return DaoFactory.CreateMainFilteringDao().RowCount<ProjectTagModel>(finalFilter);
		}

		[JsonEndpoint, RequireAuthorization]
		public object CreateProjectTag(Guid parentId, [NotEmpty] string name)
		{
			var validation = new ValidationResult();
			
			try
			{
				var dao = DaoFactory.CreateMainCrudDao();
				var tag = new ProjectTagModel
				{
					OwnerId = CurrentUser.Id,
					Name = name.TrimSafe(),
					Parent = !default(Guid).Equals(parentId) ? dao.Get<ProjectTagModel>(parentId, true) : null
				};

				SecurityService.DemandUpdate(tag, null, CurrentUser.Id, MainSession);

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<ProjectTagModel>(q => q.Where(m => 
						m.Name == tag.Name 
						&& m.Parent == tag.Parent 
						&& m.OwnerId == tag.OwnerId)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation, MainSession);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<ProjectTagModel>();
				DoUpdateProjectTag(dao, tag, affected);
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
				var dao = DaoFactory.CreateMainCrudDao();
				var tag = dao.Get<ProjectTagModel>(id, true);

				SecurityService.DemandUpdate(tag, null, CurrentUser.Id, MainSession);

				tag.Name = name.TrimSafe();

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<ProjectTagModel>(q => q.Where(m => 
						m.Name == tag.Name 
						&& m.Parent == tag.Parent
						&& m.OwnerId == tag.OwnerId 
						&& m.Id != tag.Id)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation, MainSession);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<ProjectTagModel>();
				DoUpdateProjectTag(dao, tag, affected);
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
				var dao = DaoFactory.CreateMainCrudDao();
				var tag = dao.Get<ProjectTagModel>(id, true);
				SecurityService.DemandDelete(tag, null, CurrentUser.Id, MainSession);
				var deletedIds = new HashSet<Guid>();
				DoDeleteProjectTag(dao, tag, deletedIds);
				return deletedIds;
			}
			catch (Exception e)
			{
				validation.AddErrors(_LocalizationService.MessageForException(e));
			}

			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public CustomPropertyTypeModel GetCustomPropertyType([NotEmpty] string project, [NotEmpty] Guid id)
		{
			var fdao = DaoFactory.CreateProjectFilteringDao(project);
			var fb = _FilteringService.Filter<CustomPropertyTypeModel>();
			var filter = SecurityService.ApplyReadConstraint<CustomPropertyTypeModel>(project, CurrentUser.Id,
				ProjectSession(project), fb.Where(m => m.Id == id));

			return fdao.Find<CustomPropertyTypeModel>(filter);
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

		protected void DoDeleteProjectTag(ICrudDao dao, ProjectTagModel tag, ISet<Guid> deletedIds)
		{
			if ((CurrentUser.Id != tag.OwnerId) && CurrentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			foreach (var subTag in MainSession.QueryOver<ProjectTagModel>()
					.Where(m => m.Parent.Id == tag.Id).List())
				DoDeleteProjectTag(dao, subTag, deletedIds);

			deletedIds.Add(tag.Id);
			dao.Delete(tag);
		}

		protected void DoUpdateProjectTag(ICrudDao dao, ProjectTagModel tag, ISet<ProjectTagModel> affected)
		{
			if (tag == null)
				return;

			var fullName = new StringBuilder(tag.Parent != null ? tag.Parent.FullName : string.Empty);		
			if (fullName.Length > 0)
				fullName.Append(" / ");
			fullName.Append(tag.Name);

			tag.FullName = fullName.ToString();

			dao.Store(tag);
			affected.Add(tag);

			foreach (var subTag in tag.Children.OfType<ProjectTagModel>())
				DoUpdateProjectTag(dao, subTag, affected);
		}

		#endregion
	}
}