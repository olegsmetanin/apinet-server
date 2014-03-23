using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Filters;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using NHibernate;

namespace AGO.Core.Controllers
{
	public abstract class AbstractController : AbstractService
	{
		#region Properties, fields, constructors

		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		protected readonly DaoFactory DaoFactory;

		protected readonly ISessionProviderRegistry SessionProviderRegistry;

		protected readonly ILocalizationService _LocalizationService;

		protected readonly IModelProcessingService _ModelProcessingService;

		protected readonly AuthController _AuthController;

		protected readonly ISecurityService SecurityService;

		protected AbstractController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry providerRegistry,
			DaoFactory factory)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;

			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (modelProcessingService == null)
				throw new ArgumentNullException("modelProcessingService");
			_ModelProcessingService = modelProcessingService;

			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;

			if (securityService == null)
				throw new ArgumentNullException("securityService");
			SecurityService = securityService;

			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");
			SessionProviderRegistry = providerRegistry;

			if (factory == null)
				throw new ArgumentNullException("factory");
			DaoFactory = factory;
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			var initializable = _JsonService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			initializable = _FilteringService as IInitializable;
			if (initializable != null)
				initializable.Initialize();

			_AuthController.Initialize();
		}

		#endregion

		#region Helper methods

		protected virtual UserModel CurrentUser
		{
			get { return _AuthController.CurrentUser(); }
		}

		protected virtual ISession MainSession
		{
			get { return SessionProviderRegistry.GetMainDbProvider().CurrentSession; }
		}

		protected virtual ISession ProjectSession(string project)
		{
			return SessionProviderRegistry.GetProjectProvider(project).CurrentSession;
		}

		protected virtual ProjectMemberModel UserToMember(string project, Guid userId)
		{
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentNullException("project");

			return ProjectSession(project).QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project && m.UserId == userId)
				.SingleOrDefault();
		}

		protected virtual ProjectMemberModel CurrentUserToMember(string project)
		{
			return CurrentUser != null ? UserToMember(project, CurrentUser.Id) : null;
		}

		protected IEnumerable<IModelMetadata> MetadataForModelAndRelations<TModel>()
			where TModel : IIdentifiedModel
		{
			return MetadataForModelAndRelations(null, typeof (TModel));
		}

		protected IEnumerable<IModelMetadata> MetadataForModelAndRelations<TModel>(string project)
			where TModel : IIdentifiedModel
		{
			return MetadataForModelAndRelations(project, typeof(TModel));
		}

		private IEnumerable<IModelMetadata> MetadataForModelAndRelations(string project, Type modelType)
		{
			var result = new List<IModelMetadata>();
			var processedTypes = new HashSet<Type>();

			ProcessMetadata(project, modelType, result, processedTypes);

			return result;
		}

		private void ProcessMetadata(string project, Type modelType, ICollection<IModelMetadata> result, ICollection<Type> processedTypes)
		{
			if (modelType == null || processedTypes.Contains(modelType))
				return;

			var sp = project == null
				? SessionProviderRegistry.GetMainDbProvider()
				: SessionProviderRegistry.GetProjectProvider(project);
			var metadata = sp.ModelMetadata(modelType);
			if (metadata == null)
				return;

			result.Add(metadata);
			processedTypes.Add(modelType);

			foreach (var modelProperty in metadata.ModelProperties)
				ProcessMetadata(project, modelProperty.PropertyType, result, processedTypes);
		}

		protected IEnumerable<LookupEntry> LookupEnum<TEnum>(
			string term,
			int page,
			ref IDictionary<string, LookupEntry[]> cache)
		{
			if (page > 0) return Enumerable.Empty<LookupEntry>(); //while size of enum less than defaul page size (10)

			var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (cache == null)
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache = new Dictionary<string, LookupEntry[]>();
			}
			if (!cache.ContainsKey(lang))
			{
				//no need to locking - replace with same value from another thread has no negative effect
				cache[lang] = Enum.GetValues(typeof(TEnum))
					.OfType<TEnum>() //GetValues preserve enum order, no OrderBy used
					.Select(s => new LookupEntry
					{
						Id = s.ToString(),
						Text = (_LocalizationService.MessageForType(s.GetType(), s) ?? s.ToString())
					})
					.ToArray();
			}

			if (term.IsNullOrWhiteSpace())
				return cache[lang];

			return cache[lang]
				.Where(l => l.Text.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
				.ToArray();
		}

		#endregion
	}
}