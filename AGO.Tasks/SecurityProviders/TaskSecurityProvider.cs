using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskSecurityProvider: ModuleSecurityProvider<TaskModel>
	{
		public TaskSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			if (member.ProjectCode == project) return null;
			return FilteringService.Filter<TaskModel>().Where(m => 1 == 2);
		}

		public override bool CanCreate(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			return member.IsInRole(BaseProjectRoles.Administrator) || member.IsInRole(TaskProjectRoles.Manager);
		}

		public override bool CanUpdate(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;//any member can change task
		}

		public override bool CanDelete(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			return member.IsInRole(BaseProjectRoles.Administrator) || member.IsInRole(TaskProjectRoles.Manager);
		}
	}
}