using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class ProjectMemberSecurityProvider: ModuleSecurityProvider<ProjectMemberModel>
	{
		public ProjectMemberSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return null;
		}

		private bool IsProjectAdmin(ProjectMemberModel member)
		{
			return member != null && member.IsInRole(BaseProjectRoles.Administrator);
		}

		public override bool CanCreate(ProjectMemberModel model, string project, ProjectMemberModel member, ISession session)
		{
			return IsProjectAdmin(member);
		}

		public override bool CanUpdate(ProjectMemberModel model, string project, ProjectMemberModel member, ISession session)
		{
			//prj admin or themself
			return IsProjectAdmin(member) || member.Equals(model);
		}

		public override bool CanDelete(ProjectMemberModel model, string project, ProjectMemberModel member, ISession session)
		{
			return IsProjectAdmin(member);
		}
	}
}