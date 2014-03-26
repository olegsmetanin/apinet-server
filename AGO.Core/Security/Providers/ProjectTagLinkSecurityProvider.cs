using System;
using System.Linq;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ProjectTagLinkSecurityProvider: AbstractSecurityConstraintsProvider<ProjectToTagModel>
	{

		private readonly ISessionFactory mainFactory;

		public ProjectTagLinkSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			mainFactory = providerRegistry.GetMainDbProvider().SessionFactory;
		}

		protected bool IsMember(ProjectToTagModel link, UserModel user)
		{
			return link.Project.Members.Any(m => user.Equals(m.User));
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
			//all see only self assigned tags
			return FilteringService.Filter<ProjectToTagModel>()
				.Where(m => m.Project.ProjectCode == project && m.Tag.OwnerId == user.Id);
		}

		public override bool CanCreate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//sysadminis or project members may tag projects, but only own tags
			return (user.IsAdmin || IsMember(model, user)) && user.Id == model.Tag.OwnerId;
		}

		public override bool CanUpdate(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//update not supported for tagging, only add/remove tags
			return false;
		}

		public override bool CanDelete(ProjectToTagModel model, string project, UserModel user, ISession session)
		{
			//deletes only own tags (same logic as in create)
			return (user.IsAdmin || IsMember(model, user)) && user.Id == model.Tag.OwnerId;
		}
	}
}