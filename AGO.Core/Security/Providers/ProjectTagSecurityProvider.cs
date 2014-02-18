using AGO.Core.Filters;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ProjectTagSecurityProvider: AbstractSecurityConstraintsProvider<ProjectTagModel>
	{
		public ProjectTagSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, UserModel user, ISession session)
		{
			//all can see only own project tags
			return FilteringService.Filter<ProjectTagModel>().Where(m => m.Creator.Id == user.Id);
		}

		private bool CanManage(ProjectTagModel tag, UserModel u)
		{
			//all manage only own tags (create, update or delete - no restrictions)
			return u.Equals(tag.Creator);
		}

		public override bool CanCreate(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			return CanManage(model, user) && (model.Parent == null || user.Equals(model.Parent.Creator));
		}

		public override bool CanUpdate(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			return CanManage(model, user);
		}

		public override bool CanDelete(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			return CanManage(model, user);
		}
	}
}