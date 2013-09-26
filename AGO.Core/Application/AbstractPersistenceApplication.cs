using System;
using System.Collections.Generic;
using AGO.Core.AutoMapping;
using AGO.Core.Config;
using AGO.Core.Filters;
using AGO.Core.Migration;
using AGO.Core.Model.Processing;

namespace AGO.Core.Application
{
	public abstract class AbstractPersistenceApplication : AbstractApplication, IPersistenceApplication
	{
		#region Properties, fields, constructors

		protected ISessionProvider _SessionProvider;
		public ISessionProvider SessionProvider { get { return _SessionProvider; } }

		protected IFilteringService _FilteringService;
		public IFilteringService FilteringService { get { return _FilteringService; } }

		protected IFilteringDao _FilteringDao;
		public IFilteringDao FilteringDao { get { return _FilteringDao; } }

		protected ICrudDao _CrudDao;
		public ICrudDao CrudDao { get { return _CrudDao; } }

		protected IMigrationService _MigrationService;
		public IMigrationService MigrationService { get { return _MigrationService; } }

		protected IModelProcessingService _ModelProcessingService;
		public IModelProcessingService ModelProcessingService { get { return _ModelProcessingService; } }

		#endregion

		#region Template methods

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			DoRegisterPersistence();
		}

		protected virtual void DoRegisterPersistence()
		{
			IocContainer.RegisterSingle<ISessionProvider, AutoMappedSessionFactoryBuilder>();
			IocContainer.RegisterInitializer<AutoMappedSessionFactoryBuilder>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<IFilteringService, FilteringService>();
			IocContainer.RegisterInitializer<FilteringService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Filtering_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<CrudDao, CrudDao>();
			IocContainer.RegisterInitializer<CrudDao>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^CrudDao_(.*)", KeyValueProvider)).ApplyTo(service));
			IocContainer.Register<ICrudDao>(IocContainer.GetInstance<CrudDao>);
			IocContainer.Register<IFilteringDao>(IocContainer.GetInstance<CrudDao>);

			IocContainer.RegisterSingle<IMigrationService, MigrationService>();
			IocContainer.RegisterInitializer<MigrationService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<IModelProcessingService, ModelProcessingService>();
			IocContainer.RegisterInitializer<ModelProcessingService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^ModelProcessing_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterAll<IModelValidator>(AllModelValidators);
		}

		protected virtual IEnumerable<Type> AllModelValidators
		{
			get { return new[] { typeof(AttributeValidatingModelValidator) }; }
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();

			DoInitializePersistence();
		}

		protected virtual void DoInitializePersistence()
		{
			_SessionProvider = IocContainer.GetInstance<ISessionProvider>();
			_FilteringService = IocContainer.GetInstance<IFilteringService>();
			_FilteringDao = IocContainer.GetInstance<IFilteringDao>();
			_CrudDao = IocContainer.GetInstance<ICrudDao>();
			_MigrationService = IocContainer.GetInstance<IMigrationService>();
			_ModelProcessingService = IocContainer.GetInstance<IModelProcessingService>();
		}

		#endregion
	}
}