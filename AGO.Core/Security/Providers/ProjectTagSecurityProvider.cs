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
			throw new System.NotImplementedException();
		}

		public override bool CanCreate(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			throw new System.NotImplementedException();
		}

		public override bool CanUpdate(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			throw new System.NotImplementedException();
		}

		public override bool CanDelete(ProjectTagModel model, string project, UserModel user, ISession session)
		{
			throw new System.NotImplementedException();
		}
	}
}