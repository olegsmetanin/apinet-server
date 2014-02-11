using System.Collections.Generic;
using AGO.Core.Application;
using AGO.Core.Modules;
using AGO.Core.Localization;
using AGO.Tasks.Controllers;

namespace AGO.Tasks
{
    /// <summary>
    /// Описатель модуля задач (ака задачник)
    /// </summary>
    public class ModuleDescriptor: IModuleDescriptor
    {
        public string Name
        {
            get { return "AGO.Tasks"; }
        }

        public string Alias
        {
            get { return "Tasks"; }
        }

        public int Priority
        {
            get { return 0; }
        }

		public void Register(IApplication app)
		{
		}

		public void Initialize(IApplication app)
		{
			app.LocalizationService.RegisterModuleLocalizers(GetType().Assembly);
		}

        public IEnumerable<IServiceDescriptor> Services { get; private set; }

        public ModuleDescriptor()
        {
            Services = new List<IServiceDescriptor>
            {
                new AttributedWebServiceDescriptor<DictionaryController>(this),
                new AttributedWebServiceDescriptor<TasksController>(this),
				new AttributedWebServiceDescriptor<ProjectController>(this)
            };
        }
    }
}
