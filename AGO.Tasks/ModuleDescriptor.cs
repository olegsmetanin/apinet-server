using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AGO.Core.Application;
using AGO.Core.Controllers;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Modules;
using AGO.Core.Localization;
using AGO.Core.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.Activity;
using AGO.Tasks.Processing;
using SimpleInjector;
using SimpleInjector.Advanced;
using DictionaryController = AGO.Tasks.Controllers.DictionaryController;

[assembly: InternalsVisibleTo("AGO.Tasks.Test")]

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

			var fileResRegistration = Lifestyle.Singleton.CreateRegistration(
				typeof(IFileResourceStorage),
				() => app.IocContainer.GetInstance<TasksController>(),
				app.IocContainer);
			app.IocContainer.AppendToCollection(typeof(IFileResourceStorage), fileResRegistration);

			app.IocContainer.RegisterSingle<ProjectAttributesActivityPostProcessor, ProjectAttributesActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskAttributesActivityPostProcessor, TaskAttributesActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskCustomPropertiesActivityPostProcessor, TaskCustomPropertiesActivityPostProcessor>();
			app.IocContainer.RegisterSingle<ProjectTasksRelatedActivityPostProcessor, ProjectTasksRelatedActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskAgreementsRelatedActivityPostProcessor, TaskAgreementsRelatedActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskExecutorsRelatedActivityPostProcessor, TaskExecutorsRelatedActivityPostProcessor>();
			app.IocContainer.RegisterSingle<TaskFilesRelatedActivityPostProcessor, TaskFilesRelatedActivityPostProcessor>();
			app.IocContainer.RegisterSingle<ProjectAttributesActivityViewProcessor, ProjectAttributesActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskAttributesActivityViewProcessor, TaskAttributesActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskCustomPropertiesActivityViewProcessor, TaskCustomPropertiesActivityViewProcessor>();		
			app.IocContainer.RegisterSingle<ProjectTasksRelatedActivityViewProcessor, ProjectTasksRelatedActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskAgreementsRelatedActivityViewProcessor, TaskAgreementsRelatedActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskExecutorsRelatedActivityViewProcessor, TaskExecutorsRelatedActivityViewProcessor>();
			app.IocContainer.RegisterSingle<TaskFilesRelatedActivityViewProcessor, TaskFilesRelatedActivityViewProcessor>();

			var projFactoryRegistration = Lifestyle.Singleton.CreateRegistration(
				typeof (IProjectFactory),
				typeof (TasksProjectFactory),
				app.IocContainer);
			app.IocContainer.AppendToCollection(typeof(IProjectFactory), projFactoryRegistration);
		}

		public void Initialize(IApplication app)
		{
			app.LocalizationService.RegisterModuleLocalizers(GetType().Assembly);

			var persistentApp = app as IPersistenceApplication;
			if (persistentApp == null)
				return;

			persistentApp.ModelProcessingService.RegisterModelPostProcessors(new IModelPostProcessor[]
			{
				app.IocContainer.GetInstance<ProjectAttributesActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskAttributesActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskCustomPropertiesActivityPostProcessor>(),
				app.IocContainer.GetInstance<ProjectTasksRelatedActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskAgreementsRelatedActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskExecutorsRelatedActivityPostProcessor>(),
				app.IocContainer.GetInstance<TaskFilesRelatedActivityPostProcessor>()
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
