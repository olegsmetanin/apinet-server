using System;
using System.Linq;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Configuration;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ProjectSecurityProvider: AbstractSecurityConstraintsProvider<ProjectModel>
	{
		private readonly ISessionFactory mainFactory;

		public ProjectSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry) : base(filteringService)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			mainFactory = providerRegistry.GetMainDbProvider().SessionFactory;
		}

		//because this provider mostly used in modules (in core only read and create constraints)
		//we can't use project session to obtain user
		protected override UserModel UserFromId(Guid userId, ISession session)
		{
			var s = mainFactory.OpenSession();
			try
			{
				return base.UserFromId(userId, s);
			}
			finally
			{
				s.Close();
			}
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

		private bool HasTicket(UserModel user, ISession session)
		{
			return session.QueryOver<ProjectTicketModel>().Where(m => m.User.Id == user.Id && m.Project == null).Exists();
		}

		public override bool CanCreate(ProjectModel model, string project, UserModel user, ISession session)
		{
			//admin or user, if has open ticket for creating project
			return user.IsAdmin || HasTicket(user, session);
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
