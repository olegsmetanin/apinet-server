using AGO.Core.Filters;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class CustomPropertyTypeSecurityProvider: AbstractModuleSecurityConstraintsProvider<CustomPropertyTypeModel>
	{
		public CustomPropertyTypeSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session)
		{
			return FilteringService.Filter<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project);
		}

		//don't know what else logic may be applied for core entity, that used
		//only in modules (if two project from different modules with diff security logic persistet in one db)

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