using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Core.Security.Providers;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	/// <summary>
	/// Deny changes to all except project admin (and sysadmin restricted too)
	/// </summary>
	public class TaskProjectSecurityProvider: ProjectSecurityProvider
	{
		private readonly ProjectToModuleCache p2m;

		public TaskProjectSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
			p2m = new ProjectToModuleCache(ModuleDescriptor.MODULE_CODE);
		}

		public override bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return base.AcceptChange(model, project, session) && p2m.IsProjectInHandledModule(project, session);
		}

		private bool IsProjectAdmin(ProjectModel p, UserModel u, ISession session)
		{
			var member = session.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == u.Id).SingleOrDefault();
			return member != null && member.IsInRole(BaseProjectRoles.Administrator);
		}

		//create is in core, we can't handle this here

		public override bool CanUpdate(ProjectModel model, string project, UserModel user, ISession session)
		{
			return IsProjectAdmin(model, user, session);
		}

		public override bool CanDelete(ProjectModel model, string project, UserModel user, ISession session)
		{
			return IsProjectAdmin(model, user, session);
		}
	}
}