using System.Collections.Generic;
using AGO.Core.Modules;
using AGO.System.Controllers;

namespace AGO.System
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.System"; } }

		public string Alias { get { return "System"; } }

		public int Priority { get { return 0; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }
		
		public void Register(IModuleConsumer consumer)
		{
		}

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedServiceDescriptor<UsersController>(this),
			};
		}
	}
}
