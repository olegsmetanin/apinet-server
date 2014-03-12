using System.Collections.Generic;
using AGO.Core.Application;
using AGO.Core.Config;
using AGO.Core.Controllers.Security;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Modules;
using AGO.Core.Controllers;
using AGO.Core.Security;
using AGO.Reporting.Common.Model;

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
			var di = app.IocContainer;

			app.RegisterModuleSecurityProviders(GetType().Assembly);

			di.RegisterSingle<FacebookProvider>();
			di.RegisterInitializer<FacebookProvider>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^OAuth_Facebook_(.*)", app.KeyValueProvider)).ApplyTo(service));

			di.RegisterSingle<TwitterProvider>();
			di.RegisterInitializer<TwitterProvider>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^OAuth_Twitter_(.*)", app.KeyValueProvider)).ApplyTo(service));

			di.RegisterSingle<IOAuthProviderFactory>(new OAuthProviderFactory
			{
				{OAuthProvider.Facebook, di.GetInstance<FacebookProvider>},
				{OAuthProvider.Twitter, di.GetInstance<TwitterProvider>}
			});

			di.RegisterSingle<ProjectAttributesActivityPostProcessor, ProjectAttributesActivityPostProcessor>();
		}

		public void Initialize(IApplication app)
		{
			app.SecurityService.InitializeModuleSecurityProviders(app.IocContainer);

			app.LocalizationService.RegisterModuleLocalizers(GetType().Assembly);
			app.LocalizationService.RegisterModuleLocalizers(typeof(ReportTaskState).Assembly);

			var persistentApp = app as IPersistenceApplication;
			if (persistentApp == null)
				return;

			persistentApp.ModelProcessingService.RegisterModelPostProcessors(
				new[] { app.IocContainer.GetInstance<ProjectAttributesActivityPostProcessor>() });
		}

		public ModuleDescriptor()
		{
			Services = new List<IServiceDescriptor>
			{
				new AttributedWebServiceDescriptor<DictionaryController>(this),
				new AttributedWebServiceDescriptor<AuthController>(this),
				new AttributedWebServiceDescriptor<ProjectsController>(this),
				new AttributedWebServiceDescriptor<UsersController>(this),
				new AttributedWebServiceDescriptor<ReportingController>(this)
			};
		}
	}
}