using System.Collections.Generic;
using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Model.Processing;
using AGO.Core.Modules;
using AGO.Core.Localization;
using AGO.Core.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.Activity;
using AGO.Tasks.Processing;
using SimpleInjector;
using SimpleInjector.Advanced;
using DictionaryController = AGO.Tasks.Controllers.DictionaryController;

namespace AGO.Tasks
{
    /// <summary>
    /// Описатель модуля задач (ака задачник)
    /// </summary>
    public class ModuleDescriptor: IModuleDescriptor
    {
	    public const string MODULE_CODE = "Tasks";

        public string Name
        {
            get { return "AGO." + MODULE_CODE; }
        }

        public string Alias
        {
            get { return MODULE_CODE; }
        }

        public int Priority
        {
            get { return 0; }
        }

		public void Register(IApplication app)
		{
			app.RegisterModuleSecurityProviders(GetType().Assembly);

			var registration = Lifestyle.Singleton.CreateRegistration(
				typeof(IFileResourceStorage),
				() => app.IocContainer.GetInstance<TasksController>(),
				app.IocContainer);
			app.IocContainer.AppendToCollection(typeof(IFileResourceStorage), registration);

			app.IocContainer.RegisterSingle<TaskAttributesActivityPostProcessor, TaskAttributesActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskCollectionActivityPostProcessor, TaskCollectionActivityPostProcessor>();			
			app.IocContainer.RegisterSingle<TaskAttributeActivityViewProcessor, TaskAttributeActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskCollectionActivityViewProcessor, TaskCollectionActivityViewProcessor>();
		}

		public void Initialize(IApplication app)
		{
			app.LocalizationService.RegisterModuleLocalizers(GetType().Assembly);

			var persistentApp = app as IPersistenceApplication;
			if (persistentApp == null)
				return;

			persistentApp.ModelProcessingService.RegisterModelPostProcessors(new IModelPostProcessor[]
			{
				app.IocContainer.GetInstance<TaskCollectionActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskAttributesActivityPostProcessor>()				
			});
		}

        public IEnumerable<IServiceDescriptor> Services { get; private set; }

        public ModuleDescriptor()
        {
            Services = new List<IServiceDescriptor>
            {
                new AttributedWebServiceDescriptor<DictionaryController>(this),
                new AttributedWebServiceDescriptor<TasksController>(this),
				new AttributedWebServiceDescriptor<ProjectController>(this),
				new AttributedWebServiceDescriptor<ConfigController>(this),
				new AttributedWebServiceDescriptor<ActivityController>(this)
            };
        }
    }
}
