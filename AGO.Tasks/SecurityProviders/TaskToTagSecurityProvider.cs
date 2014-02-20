using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskToTagSecurityProvider: ModuleSecurityProvider<TaskToTagModel>
	{
		public TaskToTagSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return FilteringService.Filter<TaskToTagModel>().Where(m => m.Tag.Creator.Id == member.UserId);
		}

		public override bool CanCreate(TaskToTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return model.Tag != null && model.Tag.Creator.Id == member.UserId;
		}

		public override bool CanUpdate(TaskToTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return model.Tag != null && model.Tag.Creator.Id == member.UserId;
		}

		public override bool CanDelete(TaskToTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return model.Tag != null && model.Tag.Creator.Id == member.UserId;
		}
	}
}