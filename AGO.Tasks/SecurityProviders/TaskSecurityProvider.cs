using System;
using System.Linq;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskSecurityProvider: ModuleSecurityProvider<TaskModel>
	{
		public TaskSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			//not a members can't see project data
			if (!project.Equals(member.ProjectCode, StringComparison.InvariantCultureIgnoreCase)) return FilteringService.Filter<TaskModel>().Where(m => 1 == 2);
			//executors can see only his tasks
			if (member.IsInRole(TaskProjectRoles.Executor))
				return FilteringService.Filter<TaskModel>()
					.WhereCollection(m => m.Executors)
					.Where(m => m.Executor.Id == member.Id)
					.End();
			//other roles (admins and managers) can see all tasks
			return null;
		}

		public override bool CanCreate(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			return member.IsInRole(BaseProjectRoles.Administrator) || member.IsInRole(TaskProjectRoles.Manager);
		}

		public override bool CanUpdate(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			//executors can change own tasks
			if (member.IsInRole(TaskProjectRoles.Executor))
				return model.Executors.Any(e => e.Executor.Equals(member));
			//admins and manager can change any task
			return true;
		}

		public override bool CanDelete(TaskModel model, string project, ProjectMemberModel member, ISession session)
		{
			return member.IsInRole(BaseProjectRoles.Administrator) || member.IsInRole(TaskProjectRoles.Manager);
		}
	}
}