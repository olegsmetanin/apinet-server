using System.Collections.Generic;
using AGO.Core.Modules;
using AGO.DocManagement.Controllers;

namespace AGO.DocManagement
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.DocManagement"; } }

		public string Alias { get { return "Docs"; } }

		public int Priority { get { return 0; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }

		public void Register(IModuleConsumer consumer)
		{
		}

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedServiceDescriptor<DictionaryController>(this),
				new AttributedServiceDescriptor<DocumentsController>(this)
			};
		}
	}
}
