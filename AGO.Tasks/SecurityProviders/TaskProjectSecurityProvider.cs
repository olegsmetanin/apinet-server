﻿using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security.Providers;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	/// <summary>
	/// Deny changes to all except project admin (and sysadmin restricted too)
	/// </summary>
	public sealed class TaskProjectSecurityProvider: ProjectSecurityProvider
	{
		public TaskProjectSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry) 
			: base(filteringService, providerRegistry)
		{
		}

		private bool IsProjectInTasksModule(ProjectModel model)
		{
			return model != null && model.Type != null && model.Type.Module == ModuleDescriptor.MODULE_CODE;
		}

		public override bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return base.AcceptChange(model, project, session) && IsProjectInTasksModule(model as ProjectModel);
		}

		private bool IsProjectAdmin(ProjectModel p, UserModel u, ISession session)
		{
			var member = session.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == p.ProjectCode && m.UserId == u.Id).SingleOrDefault();
			return member != null && member.IsInRole(BaseProjectRoles.Administrator);
		}

		//create is in core, we can't handle this here

		public override bool CanUpdate(ProjectModel model, string project, UserModel user, ISession session)
		{
			return IsProjectAdmin(model, user, session);
		}

		public override bool CanDelete(ProjectModel model, string project, UserModel user, ISession session)
		{
			return IsProjectAdmin(model, user, session);
		}
	}
}