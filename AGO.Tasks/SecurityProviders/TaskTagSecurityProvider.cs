using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskTagSecurityProvider: ModuleSecurityProvider<TaskTagModel>
	{
		public TaskTagSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			//all view only own tags
			return FilteringService.Filter<TaskTagModel>().Where(m => m.ProjectCode == project && m.OwnerId == member.UserId);
		}

		public bool CanManage(TaskTagModel tag, ProjectMemberModel member)
		{
			return tag.OwnerId == member.UserId;
		}

		public override bool CanCreate(TaskTagModel model, string project, ProjectMemberModel member, ISession session)
		{
			return CanManage(model, member) && (model.Parent == null || model.Parent.OwnerId == model.OwnerId);
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