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
		public IEnumerable<LookupEntry> LookupProjectTypesByName(
			[InRange(0, null)] int page,
			string term)
		{
			var query = MainSession.QueryOver<ProjectTypeModel>()
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return DaoFactory.CreateMainCrudDao().PagedQuery(query, page).LookupList(m => m.Name, null, false);
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

		#region Tags management

		private static readonly IDictionary<string, Type> tagTypes = new Dictionary<string, Type>();

		public static void RegisterTagType(string typeCode, Type tagModelType)
		{
			if (typeCode.IsNullOrWhiteSpace())
				throw new ArgumentNullException("typeCode");
			if (tagModelType == null)
				throw new ArgumentNullException("tagModelType");
			if (!typeof(TagModel).IsAssignableFrom(tagModelType))
				throw new ArgumentException("Tag type must be inheritor from TagModel", "tagModelType");

			tagTypes[typeCode.ToLowerInvariant()] = tagModelType;
		}

		private bool ResolveByTagType(string type, string project, out Type tagType, out ISession session, out ICrudDao dao)
		{
			tagType = tagTypes[type.ToLowerInvariant()];
			var useMainSession = type.Equals(ProjectTagModel.TypeCode, StringComparison.InvariantCultureIgnoreCase);
			session = useMainSession ? MainSession : ProjectSession(project);
			dao = project.IsNullOrWhiteSpace()
				? DaoFactory.CreateMainCrudDao()
				: DaoFactory.CreateProjectCrudDao(project);

			return useMainSession;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTags(
			string project,
			[NotEmpty] string type,
			string term,
			[InRange(0, null)] int page)
		{
			Type tagType;
			ISession s;
			ICrudDao dao;
			var useMain = ResolveByTagType(type, project, out tagType, out s, out dao);

			var fb = _FilteringService.Filter<TagModel>();
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
				termFilter = fb
					.WhereString(m => m.FullName).Like(term, true, true);
			IModelFilterNode projFilter = useMain
				? fb.WhereProperty(m => m.ProjectCode).Not().Exists()
				: fb.Where(m => m.ProjectCode == project);

			var filter = SecurityService.ApplyReadConstraint(tagType, project, CurrentUser.Id, s, termFilter, projFilter);

			//compilefilter is main row. call with tagType create rigth criteria, that in reality not used later in LookupModelsList and we can use generic TagModel for expressions
			var criteria = _FilteringService.CompileFilter(filter, tagType) 
				.GetExecutableCriteria(s)
				.AddOrder(Order.Asc(Projections.Property<TagModel>(m => m.FullName)));
			return dao.PagedCriteria(criteria, page).LookupModelsList<TagModel>(m => m.FullName);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<TagModel> GetTags(
			string project,
			[NotEmpty] string type,
			[InRange(0, null)] int page)
		{
			Type tagType;
			ISession s;
			ICrudDao dao;
			ResolveByTagType(type, project, out tagType, out s, out dao);

			var filter = SecurityService.ApplyReadConstraint(tagType, project, CurrentUser.Id, s);
			var criteria = _FilteringService.CompileFilter(filter, tagType).GetExecutableCriteria(s);
			var order = Order.Asc(Projections.Property<TagModel>(m => m.FullName));
			return dao.PagedCriteria(criteria, page).AddOrder(order).List<TagModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetTagsCount(string project, [NotEmpty] string type)
		{
			Type tagType;
			ISession s;
			ICrudDao dao;
			ResolveByTagType(type, project, out tagType, out s, out dao);

			var filter = SecurityService.ApplyReadConstraint(tagType, project, CurrentUser.Id, s);
			var criteria = _FilteringService.CompileFilter(filter, tagType).GetExecutableCriteria(s);

			return dao.RowCount<TagModel>(criteria);
		}

		[JsonEndpoint, RequireAuthorization]
		public object CreateTag(string project, [NotEmpty] string type, Guid parentId, [NotEmpty] string name)
		{
			var validation = new ValidationResult();
			Type tagType;
			ISession s;
			ICrudDao dao;
			ResolveByTagType(type, project, out tagType, out s, out dao);
			try
			{
				var tag = (TagModel)Activator.CreateInstance(tagType);
				tag.ProjectCode = project;
				tag.OwnerId = CurrentUser.Id;
				tag.Name = name.TrimSafe();
				tag.Parent = !default(Guid).Equals(parentId) ? dao.Get<TagModel>(parentId, true, tagType) : null;

				SecurityService.DemandUpdate(tag, project, CurrentUser.Id, s);

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<TagModel>(q => q.Where(m =>
						m.GetType() == tagType 
						&& m.Name == tag.Name 
						&& m.Parent == tag.Parent 
						&& m.OwnerId == tag.OwnerId)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation, s);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<TagModel>();
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
		public object UpdateTag(string project, [NotEmpty] string type, [NotEmpty] Guid id, [NotEmpty] string name)
		{
			var validation = new ValidationResult();
			Type tagType;
			ISession s;
			ICrudDao dao;
			ResolveByTagType(type, project, out tagType, out s, out dao);

			try
			{
				var tag = dao.Get<TagModel>(id, true, tagType);

				SecurityService.DemandUpdate(tag, null, CurrentUser.Id, s);

				tag.Name = name.TrimSafe();

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<TagModel>(q => q.Where(m => 
						m.GetType() == tagType 
						&& m.Name == tag.Name 
						&& m.Parent == tag.Parent
						&& m.OwnerId == tag.OwnerId 
						&& m.Id != tag.Id)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(tag, validation, MainSession);
				if (!validation.Success)
					return validation;

				var affected = new HashSet<TagModel>();
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
		public object DeleteTag(string project, [NotEmpty] string type, [NotEmpty] Guid id)
		{
			var validation = new ValidationResult();
			Type tagType;
			ISession s;
			ICrudDao dao;
			ResolveByTagType(type, project, out tagType, out s, out dao);

			try
			{
				var tag = dao.Get<TagModel>(id, true, tagType);
				SecurityService.DemandDelete(tag, null, CurrentUser.Id, s);
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

		#endregion

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

		protected void DoDeleteProjectTag(ICrudDao dao, TagModel tag, ISet<Guid> deletedIds)
		{
			if ((CurrentUser.Id != tag.OwnerId) && CurrentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			foreach (var subTag in tag.Children)
				DoDeleteProjectTag(dao, subTag, deletedIds);

			deletedIds.Add(tag.Id);
			dao.Delete(tag);
		}

		protected void DoUpdateProjectTag(ICrudDao dao, TagModel tag, ISet<TagModel> affected)
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

			foreach (var subTag in tag.Children)
				DoUpdateProjectTag(dao, subTag, affected);
		}

		#endregion
	}
}