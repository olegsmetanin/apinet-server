using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Core.Security.Providers;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskProjectTagLinkSecurityProvider: ProjectTagLinkSecurityProvider
	{
		private readonly ProjectToModuleCache p2m;

		public TaskProjectTagLinkSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
			p2m = new ProjectToModuleCache(ModuleDescriptor.MODULE_CODE);
		}

		public override bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return base.AcceptChange(model, project, session) && p2m.IsProjectInHandledModule(project, session);
		}

		public override bool CanCreate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			return IsMember(model, user) && user.Equals(model.Tag.Creator);
		}

		public override bool CanDelete(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			return IsMember(model, user) && user.Equals(model.Tag.Creator);
		}
	}
}