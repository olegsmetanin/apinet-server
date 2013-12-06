using System.Collections.Generic;
using AGO.Core.Application;
using AGO.Core.Localization;
using AGO.Core.Modules;
using AGO.Core.Controllers;

namespace AGO.Core
{
	public class ModuleDescriptor : IModuleDescriptor
	{
		public string Name { get { return "AGO.Core"; } }

		public string Alias { get { return "Core"; } }

		public int Priority { get { return int.MinValue; } }

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
				new AttributedWebServiceDescriptor<AuthController>(this),
				new AttributedWebServiceDescriptor<ProjectsController>(this),
				new AttributedWebServiceDescriptor<UsersController>(this)
			};
		}
	}
}
