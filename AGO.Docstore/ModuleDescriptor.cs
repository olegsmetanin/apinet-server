using System.Collections.Generic;
using AGO.Hibernate.Modules;
using AGO.Docstore.Controllers;

namespace AGO.Docstore
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.Docstore"; } }

		public string Alias { get { return "Core"; } }

		public int Priority { get { return int.MinValue; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedServiceDescriptor<DictionaryController>(this),
				new AttributedServiceDescriptor<DocumentsController>(this),
				new AttributedServiceDescriptor<ProjectsController>(this)
			};
		}
	}
}
