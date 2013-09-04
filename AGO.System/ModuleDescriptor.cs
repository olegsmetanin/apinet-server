using System.Collections.Generic;
using AGO.Core.Controllers;
using AGO.Core.Modules;

namespace AGO.System
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.System"; } }

		public string Alias { get { return "System"; } }

		public int Priority { get { return 0; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
			};
		}
	}
}
