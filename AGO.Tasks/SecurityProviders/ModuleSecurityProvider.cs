using AGO.Core.Filters;
using AGO.Core.Security;

namespace AGO.Tasks.SecurityProviders
{
	public abstract class ModuleSecurityProvider<TModel>: AbstractModuleSecurityConstraintsProvider<TModel>
	{
		protected ModuleSecurityProvider(IFilteringService filteringService) : base(filteringService)
		{
		}

		protected override string Module
		{
			get { return ModuleDescriptor.MODULE_CODE; }
		}
	}
}