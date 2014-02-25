using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	//TODO what logic should be there?
	public class TaskFileSecurityProvider: ModuleSecurityProvider<TaskFileModel>
	{
		public TaskFileSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return null;
		}

		public override bool CanCreate(TaskFileModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}

		public override bool CanUpdate(TaskFileModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}

		public override bool CanDelete(TaskFileModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}
	}
}