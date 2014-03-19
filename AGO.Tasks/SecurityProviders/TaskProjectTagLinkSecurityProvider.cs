using System;
using AGO.Core;
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

		public TaskProjectTagLinkSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry) : base(filteringService)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			p2m = new ProjectToModuleCache(ModuleDescriptor.MODULE_CODE, providerRegistry.GetMainDbProvider().SessionFactory);
		}

		public override bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return base.AcceptChange(model, project, session) && p2m.IsProjectInHandledModule(project);
		}

		public override bool CanCreate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			return IsMember(model, user) && user.Id == model.Tag.OwnerId;
		}

		public override bool CanDelete(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			return IsMember(model, user) && user.Id == model.Tag.OwnerId;
		}
	}
}
