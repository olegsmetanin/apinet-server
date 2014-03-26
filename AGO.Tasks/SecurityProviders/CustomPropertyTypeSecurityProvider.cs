using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Tasks.SecurityProviders
{
	public class CustomPropertyTypeSecurityProvider: ModuleSecurityProvider<CustomPropertyTypeModel>
	{
		public CustomPropertyTypeSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry) 
			: base(filteringService, providerRegistry)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return FilteringService.Filter<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project);
		}

		public override bool CanCreate(CustomPropertyTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}

		public override bool CanUpdate(CustomPropertyTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}

		public override bool CanDelete(CustomPropertyTypeModel model, string project, ProjectMemberModel member, ISession session)
		{
			return true;
		}
	}
}