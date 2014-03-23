using System;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class TaskAgreementSecurityProvider: ModuleSecurityProvider<TaskAgreementModel>
	{
		public TaskAgreementSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return null;
		}

		public override bool CanCreate(TaskAgreementModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}

		private bool IsAdminOrMgrAgreemer (TaskAgreementModel model, ProjectMemberModel member)
		{
			Func<bool> isAdmin = () => member.IsInRole(BaseProjectRoles.Administrator);
			Func<bool> isManager = () => member.IsInRole(TaskProjectRoles.Manager);
			Func<bool> isAgreemer = () => member.Equals(model.Agreemer);
			
			return isAdmin() || (isManager() && isAgreemer());
		}

		public override bool CanUpdate(TaskAgreementModel model, string project, ProjectMemberModel member, ISession session)
		{
			//change agreement (typycally agree or revoke agreement) can proj admins or manager, that
			//is agreemer in this argeement
			return IsAdminOrMgrAgreemer(model, member);
		}

		public override bool CanDelete(TaskAgreementModel model, string project, ProjectMemberModel member, ISession session)
		{
			return IsAdminOrMgrAgreemer(model, member);
		}
	}
}