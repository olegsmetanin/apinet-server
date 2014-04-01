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
using AGO.Core.Model;
using AGO.Core.Model.Dictionary;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
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
				DoUpdateHierarchyItem(dao, tag, affected);
				return tag;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when create tag", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
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

				SecurityService.DemandUpdate(tag, project, CurrentUser.Id, s);

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
				DoUpdateHierarchyItem(dao, tag, affected);
				return affected;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when update tag", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
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
				SecurityService.DemandDelete(tag, project, CurrentUser.Id, s);
				var deletedIds = new HashSet<Guid>();
				DoDeleteHierarchyItem(dao, tag, deletedIds);
				return deletedIds;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when delete tag", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
			}

			return validation;
		}

		#endregion

		#region Custom property type management

		private static IDictionary<string, LookupEntry[]> valueTypes;
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupCustomPropertyValueTypes(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<CustomPropertyValueType>(term, page, ref valueTypes);
		}
			
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupCustomPropertyTypes(
			[NotEmpty] string project,
			[InRange(0, null)] int page,
			string term)
		{
			var fb = _FilteringService.Filter<CustomPropertyTypeModel>();
			var projFilter = fb.Where(m => m.ProjectCode == project);
			IModelFilterNode termFilter = null;
			if (!term.IsNullOrWhiteSpace())
				termFilter = fb.WhereString(m => m.FullName).Like(term, true, true);

			var filter = SecurityService.ApplyReadConstraint<CustomPropertyTypeModel>(project, CurrentUser.Id,
				ProjectSession(project), projFilter, termFilter);

			var criteria = _FilteringService.CompileFilter(filter, typeof (CustomPropertyTypeModel))
				.GetExecutableCriteria(ProjectSession(project))
				.AddOrder(Order.Asc(Projections.Property<CustomPropertyTypeModel>(m => m.FullName)));

			return DaoFactory.CreateProjectCrudDao(project).PagedCriteria(criteria, page).LookupModelsList<CustomPropertyTypeModel>(m => m.FullName);
		}

		[JsonEndpoint, RequireAuthorization]
		public CustomPropertyTypeModel GetCustomPropertyType([NotEmpty] string project, [NotEmpty] Guid id)
		{
			var fdao = DaoFactory.CreateProjectFilteringDao(project);
			var fb = _FilteringService.Filter<CustomPropertyTypeModel>();
			var filter = SecurityService.ApplyReadConstraint<CustomPropertyTypeModel>(project, CurrentUser.Id,
				ProjectSession(project), fb.Where(m => m.ProjectCode == project && m.Id == id));

			return fdao.Find<CustomPropertyTypeModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<CustomPropertyTypeModel> GetCustomPropertyTypes(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			[InRange(0, null)] int page)
		{
			var projectPredicate = _FilteringService.Filter<CustomPropertyTypeModel>().Where(m => m.ProjectCode == project);
			filter.Add(projectPredicate);
			var predicate = SecurityService.ApplyReadConstraint<CustomPropertyTypeModel>(project, CurrentUser.Id, ProjectSession(project), 
				filter.ToArray());

			return DaoFactory.CreateProjectFilteringDao(project)
				.List<CustomPropertyTypeModel>(predicate, page, sorters)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetCustomPropertyTypesCount([NotEmpty] string project, [NotNull] ICollection<IModelFilterNode> filter)
		{
			var projectPredicate = _FilteringService.Filter<CustomPropertyTypeModel>().Where(m => m.ProjectCode == project);
			filter.Add(projectPredicate);
			var predicate = SecurityService.ApplyReadConstraint<CustomPropertyTypeModel>(project, CurrentUser.Id, ProjectSession(project), 
				filter.ToArray());

			return DaoFactory.CreateProjectFilteringDao(project).RowCount<CustomPropertyTypeModel>(predicate);
		}

		[JsonEndpoint, RequireAuthorization]
		public object CreateCustomPropertyType([NotEmpty] string project, [NotNull] CustomPropertyTypeModel model)
		{
			var validation = new ValidationResult();
			try
			{
				var dao = DaoFactory.CreateProjectCrudDao(project);
				var newParamType = new CustomPropertyTypeModel
				{
					ProjectCode = project,
					Creator = CurrentUserToMember(project),
					CreationTime = DateTime.UtcNow,
					Parent = model.ParentId.HasValue ? dao.Get<CustomPropertyTypeModel>(model.ParentId, true) : null,
					Name = model.Name,
					ValueType = model.ValueType,
					Format = model.Format
				};

				SecurityService.DemandUpdate(newParamType, project, CurrentUser.Id, ProjectSession(project));

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<CustomPropertyTypeModel>(q => q.Where(m =>
					m.ProjectCode == project
					&& m.Name == newParamType.Name
					&& m.Parent == newParamType.Parent)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(newParamType, validation, ProjectSession(project));
				if (!validation.Success)
					return validation;

				var affected = new HashSet<CustomPropertyTypeModel>();
				DoUpdateHierarchyItem(dao, newParamType, affected);
				return newParamType;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when create custom property type", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
			}
			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public object UpdateCustomPropertyType([NotEmpty] string project, [NotNull] PropChangeDTO data)
		{
			if (data.Prop.IsNullOrWhiteSpace())
				throw new ArgumentException("No prop name for update found", "data");

			var validation = new ValidationResult();
			try
			{
				var dao = DaoFactory.CreateProjectCrudDao(project);
				var paramType = dao.Get<CustomPropertyTypeModel>(data.Id, true);

				SecurityService.DemandUpdate(paramType, project, CurrentUser.Id, ProjectSession(project));

				var affected = new HashSet<CustomPropertyTypeModel>();
				switch (data.Prop)
				{
					case "Name":
						paramType.Name = data.Value as string;
						break;
					case "Format":
						paramType.Format = data.Value as string;
						break;
					default:
						throw new InvalidOperationException(string.Format("Unsupported prop for update: '{0}'", data.Prop));
				}

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<CustomPropertyTypeModel>(q => q.Where(m =>
					m.ProjectCode == project
					&& m.Name == paramType.Name
					&& m.Parent == paramType.Parent
					&& m.Id != paramType.Id)))
				{
					validation.AddFieldErrors("Name", _LocalizationService.MessageForException(new MustBeUniqueException()));
				}

				_ModelProcessingService.ValidateModelSaving(paramType, validation, ProjectSession(project));
				if (!validation.Success)
					return validation;

				if ("Name".Equals(data.Prop))
					DoUpdateHierarchyItem(dao, paramType, affected);
				else
					affected.Add(paramType);

				dao.Store(paramType);

				return affected;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when update custom property type", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
			}
			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public object DeleteCustomPropertyType([NotEmpty] string project, [NotEmpty] Guid id)
		{
			var validation = new ValidationResult();

			try
			{
				var dao = DaoFactory.CreateProjectCrudDao(project);
				var paramType = dao.Get<CustomPropertyTypeModel>(id, true);
				SecurityService.DemandDelete(paramType, project, CurrentUser.Id, ProjectSession(project));

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (dao.Exists<CustomPropertyInstanceModel>(q => q.Where(m => m.PropertyType == paramType)))
					throw new CannotDeleteReferencedItemException();

				var deletedIds = new HashSet<Guid>();
				DoDeleteHierarchyItem(dao, paramType, deletedIds);
				return deletedIds;
			}
			catch (AbstractApplicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Log.Error("Error when delete custom property type", ex);
				validation.AddErrors(_LocalizationService.MessageForException(ex));
			}
			return validation;
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> CustomPropertyMetadata()
		{
			return MetadataForModelAndRelations<CustomPropertyTypeModel>().Concat(
				MetadataForModelAndRelations<CustomPropertyInstanceModel>());
		}

		#endregion

		#endregion

		#region Helper methods

		protected void DoUpdateHierarchyItem<TModel>(ICrudDao dao, TModel item, ISet<TModel> affected) 
			where TModel: class, IHierarchicalDictionaryItemModel<TModel>, IIdentifiedModel
		{
			if (item == null)
				return;

			var fullName = new StringBuilder(item.Parent != null ? item.Parent.FullName : string.Empty);
			if (fullName.Length > 0)
				fullName.Append(" / ");
			fullName.Append(item.Name);

			item.FullName = fullName.ToString();

			dao.Store(item);
			affected.Add(item);

			foreach (var subItem in item.Children)
				DoUpdateHierarchyItem(dao, subItem, affected);
		}

		protected void DoDeleteHierarchyItem<TModel>(ICrudDao dao, TModel item, ISet<Guid> deletedIds)
			where TModel : class, IHierarchicalDictionaryItemModel<TModel>, IIdentifiedModel
		{
			foreach (var subItem in item.Children)
				DoDeleteHierarchyItem(dao, subItem, deletedIds);

			deletedIds.Add(item.Id);
			dao.Delete(item);
		}

		#endregion
	}
}