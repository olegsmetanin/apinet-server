using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskTimelogSecurityProvider: ModuleSecurityProvider<TaskTimelogEntryModel>
	{
		public TaskTimelogSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return null;
		}

		public override bool CanCreate(TaskTimelogEntryModel model, string project, ProjectMemberModel member, ISession session)
		{
			//only myself and must be task executor
			return model.Member != null && model.Member.Equals(member) 
				&& model.Task != null && model.Task.IsExecutor(member);
		}

		public override bool CanUpdate(TaskTimelogEntryModel model, string project, ProjectMemberModel member, ISession session)
		{
			//myself or proj admin
			return (model.Member != null && model.Member.Equals(member))
				|| member.IsInRole(BaseProjectRoles.Administrator);
		}

		public override bool CanDelete(TaskTimelogEntryModel model, string project, ProjectMemberModel member, ISession session)
		{
			//myself or proj admin
			return (model.Member != null && model.Member.Equals(member)) 
				|| member.IsInRole(BaseProjectRoles.Administrator);
		}
	}
}
