using System.Collections.Generic;
using AGO.Core.Application;
using AGO.Core.Localization;
using AGO.Core.Modules;
using AGO.Home.Controllers;

namespace AGO.Home
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.Home"; } }

		public string Alias { get { return "Home"; } }

		public int Priority { get { return 0; } }

		public IEnumerable<IServiceDescriptor> Services { get; private set; }

		public void Register(IApplication app)
		{
		}

		public void Initialize(IApplication app)
		{
			app.LocalizationService.RegisterModuleLocalizers(GetType().Assembly);
		}

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedWebServiceDescriptor<DictionaryController>(this),
				new AttributedWebServiceDescriptor<ProjectsController>(this),
				new AttributedWebServiceDescriptor<ConfigController>(this),
				new AttributedWebServiceDescriptor<UsersController>(this)
			};
		}
	}
}
