using System.Collections.Generic;
using AGO.Core.Modules;
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

        public IEnumerable<IServiceDescriptor> Services { get; private set; }

        public ModuleDescriptor()
        {
            Services = new List<IServiceDescriptor>
            {
                new AttributedServiceDescriptor<DictionaryController>(this),
                new AttributedServiceDescriptor<TasksController>(this)
            };
        }
    }
}
