using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;

namespace AGO.Tasks.Controllers
{
	//TODO needs abstraction in core, that require this controller (optional, some modules may does not support configuration)
	public class ConfigController: AbstractController
	{
		public ConfigController(
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

		[JsonEndpoint, RequireAuthorization]
		public object Configuration([NotEmpty] string project)
		{
			if (!DaoFactory.CreateMainCrudDao().Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			var member = CurrentUserToMember(project);
			if (member == null)
				throw new NoSuchProjectMemberException();

			return new
			{
				current = TaskProjectRoles.RoleToLookupEntry(member.CurrentRole, _LocalizationService),
				memberRoles = TaskProjectRoles.Roles(_LocalizationService, member.Roles)
				//TODO other module config values, needed by app
			};
		}
	}
}