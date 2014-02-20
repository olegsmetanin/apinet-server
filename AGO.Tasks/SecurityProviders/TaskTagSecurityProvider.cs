using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskTagSecurityProvider: ModuleSecurityProvider<TaskTagModel>
	{
		public TaskTagSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			//all view only own tags
			return FilteringService.Filter<TaskTagModel>().Where(m => m.Creator.Id == member.UserId);
		}

		public bool CanManage(TaskTagModel tag, ProjectMemberModel member)
		{
			//TODO no links to users table, remove Creator from tags
			return tag.Creator.Id == member.UserId;
		}

		public override bool CanCreate(TaskTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(model, member) && (model.Parent == null || model.Parent.Creator.Equals(model.Creator));
		}

		public override bool CanUpdate(TaskTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(model, member);
		}

		public override bool CanDelete(TaskTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(model, member);
		}
	}
}