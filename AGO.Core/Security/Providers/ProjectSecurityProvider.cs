using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ProjectSecurityProvider: AbstractSecurityConstraintsProvider<ProjectModel>
	{
		public ProjectSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, UserModel user, ISession session)
		{
			//admins view all projects
			if (user.IsAdmin) return null;
			//members view public (visible for all) or projects where participated
			return FilteringService.Filter<ProjectModel>().Or()
				.Where(m => m.VisibleForAll)
				.WhereCollection(m => m.Members).Where(m => m.User.Id == user.Id).End();
		}

		public override bool CanCreate(ProjectModel model, string project, UserModel user, ISession session)
		{
			return user.IsAdmin;
		}

		public override bool CanUpdate(ProjectModel model, string project, UserModel user, ISession session)
		{
			//modules may register own providers and strengthen update check
			return user.IsAdmin || model.Members.Any(m => user.Equals(m.User));
		}

		public override bool CanDelete(ProjectModel model, string project, UserModel user, ISession session)
		{
			//?? may be project admins can delete project
			return user.IsAdmin;
		}
	}
}