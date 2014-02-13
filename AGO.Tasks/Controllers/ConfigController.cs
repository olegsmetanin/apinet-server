using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Modules.Attributes;

namespace AGO.Tasks.Controllers
{
	//TODO needs abstraction in core, that require this controller (optional, some modules may does not support configuration)
	public class ConfigController: AbstractController
	{
		public ConfigController(
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

		[JsonEndpoint, RequireAuthorization]
		public object Configuration([NotEmpty] string project)
		{
			var p = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));
			if (p == null)
				throw new NoSuchProjectException();
			var user = _AuthController.CurrentUser();
			var member = _CrudDao.Find<ProjectMemberModel>(q => q.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == user.Id));
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