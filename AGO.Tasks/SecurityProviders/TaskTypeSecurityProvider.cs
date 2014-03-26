using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskTypeSecurityProvider: ModuleSecurityProvider<TaskTypeModel>
	{
		public TaskTypeSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			if (member.ProjectCode == project) return null;
			return FilteringService.Filter<TaskTypeModel>().Where(m => 1 == 2);
		}
		private static bool CanManage(ProjectMemberModel member)
		{
			return member.IsInRole(BaseProjectRoles.Administrator) || member.IsInRole(TaskProjectRoles.Manager);
		}

		public override bool CanCreate(TaskTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(member);
		}

		public override bool CanUpdate(TaskTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(member);
		}

		public override bool CanDelete(TaskTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(member);
		}
	}
}