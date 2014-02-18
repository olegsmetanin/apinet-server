using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ProjectTagLinkSecurityProvider: AbstractSecurityConstraintsProvider<ProjectToTagModel>
	{
		public ProjectTagLinkSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		private bool IsMember(ProjectToTagModel link, UserModel user)
		{
			return link.Project.Members.Any(m => user.Equals(m.User));
		}

		public override IModelFilterNode ReadConstraint(string project, UserModel user, ISession session)
		{
			return null;//no restrictions, implemented in ProjectViewModel in other way
			//all see only self assigned tags
		}

		public override bool CanCreate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//sysadminis or project members may tag projects, but only own tags
			return (user.IsAdmin || IsMember(model, user)) && user.Equals(model.Creator);
		}

		public override bool CanUpdate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//update not supported for tagging, only add/remove tags
			return false;
		}

		public override bool CanDelete(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//deletes only own tags (same logic as in create)
			return (user.IsAdmin || IsMember(model, user)) && user.Equals(model.Creator);
		}
	}
}