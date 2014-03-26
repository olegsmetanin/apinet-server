using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using AGO.Core.Config;
using AGO.Core.DataAccess;
using AGO.Core.DataAccess.DbConfigurator;
using AGO.Core.Filters;
using AGO.Core.Migration;
using AGO.Core.Model.Processing;
using AGO.Core.Notification;
using AGO.WorkQueue;

namespace AGO.Core.Application
{
	public abstract class AbstractPersistenceApplication : AbstractApplication, IPersistenceApplication
	{
		#region Properties, fields, constructors

		public ISessionProviderRegistry SessionProviderRegistry { get; private set; }

		protected IFilteringService _FilteringService;
		public IFilteringService FilteringService { get { return _FilteringService; } }

		public DaoFactory DaoFactory { get; private set; }

		protected IMigrationService _MigrationService;
		public IMigrationService MigrationService { get { return _MigrationService; } }

		protected IModelProcessingService _ModelProcessingService;
		public IModelProcessingService ModelProcessingService { get { return _ModelProcessingService; } }

		protected IList<Type> _TestDataServices = new List<Type>();
		public IList<Type> TestDataServices { get { return _TestDataServices; } }

		public string MasterConnectionString
		{
			get { return KeyValueProvider.Value("Persistence_MasterConnectionString"); }
		}

		public DbConfiguratorFactory DbConfiguratorFactory { get; private set; }

		public void CreateProjectDatabase(string host, string dbName, string module)
		{
			if (host.IsNullOrWhiteSpace())
				throw new ArgumentNullException("host");
			if (dbName.IsNullOrWhiteSpace())
				throw new ArgumentNullException("dbName");
			if (module.IsNullOrWhiteSpace())
				throw new ArgumentNullException("module");

			if (ModuleDescriptors.All(m => m.Alias != module))
				throw new ArgumentException(string.Format("Invalid module code: {0}", module), "module");

			string provider;
			string loginName;
			string loginPwd;
			string notUsed;
			var configurator = CreateDbConfigurator(out provider, out notUsed, out loginName, out loginPwd);
			configurator.CreateProjectDatabase(host, dbName, loginName, loginPwd);
			var projConnectionString = configurator.MakeConnectionString(host, dbName, 
				SessionProviderRegistry.GetMainDbProvider().ConnectionString);//use this cs instead of master because of created schema ownership must be to ago_user, not postgres
			var now = DateTime.UtcNow;
			var version = new Version(now.Year, now.Month, now.Day, 99);
			_MigrationService.MigrateUp(
				provider,
				projConnectionString,
				ModuleDescriptors.Where(m => m.Alias == ModuleDescriptor.MODULE_CODE || m.Alias == module).Select(d => d.GetType().Assembly),
				version);
		}

		public INotificationService NotificationService { get; protected set; }

		public IWorkQueue WorkQueue { get; protected set; }

		#endregion

		#region Template methods

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			DoRegisterPersistence();
			DoRegisterNotification();
			DoRegisterWorkQueue();
		}

		protected virtual void DoRegisterPersistence()
		{
			IocContainer.RegisterSingle<DbConfiguratorFactory>();

			IocContainer.RegisterSingle<ISessionProviderRegistry, SessionProviderRegistry>();
			IocContainer.RegisterInitializer<SessionProviderRegistry>(service =>
				service.Initialize(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)));

			IocContainer.RegisterSingle<IFilteringService, FilteringService>();
			IocContainer.RegisterInitializer<FilteringService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Filtering_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<DaoFactory>();

			IocContainer.RegisterSingle<IMigrationService, MigrationService>();

			IocContainer.RegisterSingle<IModelProcessingService, ModelProcessingService>();
			IocContainer.RegisterInitializer<ModelProcessingService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^ModelProcessing_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterAll<IModelValidator>(AllModelValidators);
			IocContainer.RegisterAll<IModelPostProcessor>(AllModelPostProcessors);
		}		

		protected virtual void DoRegisterNotification()
		{
			IocContainer.RegisterSingle(typeof(INotificationService), NotificationServiceType);
			IocContainer.RegisterInitializer<NotificationService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Notification_(.*)", KeyValueProvider)).ApplyTo(service));
		}

		protected virtual Type NotificationServiceType
		{
			get { return typeof(NotificationService); }
		}

		protected virtual void DoRegisterWorkQueue()
		{
			IocContainer.RegisterSingle<IWorkQueue>(() =>
			{
				var schema = new PostgreSqlWorkQueue.Schema
				{
					Table = "\"Core\".\"WorkQueue\"",
					TaskTypeColumn = "\"TaskType\"",
					TaskIdColumn = "\"TaskId\"",
					ProjectColumn = "\"Project\"",
					UserIdColumn = "\"User\"",
					CreateDateColumn = "\"CreateDate\"",
					PriorityTypeColumn = "\"PriorityType\"",
					UserPriorityColumn = "\"UserPriority\""
				};
				var spr = IocContainer.GetInstance<ISessionProviderRegistry>();
				return new PostgreSqlWorkQueue(spr.GetMainDbProvider().ConnectionString, schema);
			});
		}

		protected virtual IEnumerable<Type> AllModelValidators
		{
			get { return new[] { typeof(AttributeValidatingModelValidator) }; }
		}

		protected virtual IEnumerable<Type> AllModelPostProcessors
		{
			get { return Enumerable.Empty<Type>(); }
		}

		protected override void DoRegisterModules(IList<Modules.IModuleDescriptor> moduleDescriptors)
		{
			base.DoRegisterModules(moduleDescriptors);

			foreach (var type in moduleDescriptors.SelectMany(moduleDescriptor => moduleDescriptor.GetType().Assembly.GetExportedTypes()
					.Where(t => t.IsClass && t.IsPublic && !t.IsAbstract && typeof(ITestDataService).IsAssignableFrom(t))))
				_TestDataServices.Add(type);
			
			IocContainer.RegisterAll<ITestDataService>(_TestDataServices);
		}

		protected override void DoInitializeCoreServices()
		{
			base.DoInitializeCoreServices();

			DoInitializePersistence();
			DoInitializeNotification();
			DoInitializeWorkQueue();
		}

		protected virtual void DoInitializePersistence()
		{
			DbConfiguratorFactory = IocContainer.GetInstance<DbConfiguratorFactory>();
			SessionProviderRegistry = IocContainer.GetInstance<ISessionProviderRegistry>();
			_FilteringService = IocContainer.GetInstance<IFilteringService>();
			DaoFactory = IocContainer.GetInstance<DaoFactory>();
			_MigrationService = IocContainer.GetInstance<IMigrationService>();
			_ModelProcessingService = IocContainer.GetInstance<IModelProcessingService>();
		}

		protected virtual void DoInitializeNotification()
		{
			NotificationService = IocContainer.GetInstance<INotificationService>();
		}

		protected virtual void DoInitializeWorkQueue()
		{
			WorkQueue = IocContainer.GetInstance<IWorkQueue>();
		}

		protected IDbConfigurator CreateDbConfigurator(out string provider, out string dbName, out string login, out string pwd)
		{
			var config = new KeyValueConfigurableDictionary();
			new KeyValueConfigProvider(new RegexKeyValueProvider("^Persistence_(.*)", KeyValueProvider)).ApplyTo(config);

			provider = config.GetConfigProperty("ProviderName").TrimSafe();
			if (provider.IsNullOrEmpty())
				throw new Exception("ProviderName is empty");

			dbName = config.GetConfigProperty("DatabaseName").TrimSafe();
			if (dbName.IsNullOrEmpty())
				throw new Exception("DatabaseName is empty");

			login = config.GetConfigProperty("LoginName").TrimSafe();
			if (login.IsNullOrEmpty())
				throw new Exception("LoginName is empty");

			pwd = config.GetConfigProperty("LoginPwd").TrimSafe();
			if (pwd.IsNullOrEmpty())
				throw new Exception("LoginPwd is empty");

			//because this method may be called before initialize, check in factory already initialized
			return (DbConfiguratorFactory ?? new DbConfiguratorFactory()).CreateConfigurator(provider, MasterConnectionString);
		}

		protected virtual void DoCreateDatabase()
		{
			string provider;
			string databaseName;
			string loginName;
			string loginPwd;
			var configurator = CreateDbConfigurator(out provider, out databaseName, out loginName, out loginPwd);
			configurator.CreateMasterDatabase(databaseName, loginName, loginPwd);
		}

		//TODO create mssqldbconfigurator
		private static readonly Dictionary<string, string> CreateDbScripts = new Dictionary<string, string>
		{
			{"System.Data.SqlClient", @"
				IF EXISTS(SELECT name FROM sys.databases WHERE name = '{0}') BEGIN
					ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
					DROP DATABASE [{0}]
				END
				GO

				declare @Path nvarchar(500)
				set @Path = (select physical_name from sys.master_files WHERE name = 'master' AND type_desc ='ROWS')
				set @Path = (select SUBSTRING(@Path, 1, CHARINDEX('master.mdf', LOWER(@Path)) - 1))
				EXEC('CREATE DATABASE [{0}] ON PRIMARY 
				( NAME = N''{0}'', FILENAME = N''' + @Path + '{0}.mdf'', SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
					LOG ON 
				( NAME = N''{0}_log'', FILENAME = N''' + @Path + '{0}_log.ldf'', SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)')
				GO

				if not exists(select 1 from sys.sql_logins where name = N'{1}') begin
					BEGIN TRY
						create login [{1}]
						with password=N'{2}', default_database=[master], check_expiration=off, check_policy=off
						alter login [{1}] enable
					END TRY
					BEGIN CATCH
					END CATCH
				end
				go

				use [{0}]
				go

				if not exists(select 1 from sys.database_principals where name = N'{1}') begin
					BEGIN TRY
						create user [{1}] for login [{1}] with default_schema=[dbo]
						exec sp_addrolemember N'db_owner', N'{1}'
					END TRY
					BEGIN CATCH
					END CATCH
				end"}
		};

		protected virtual void DoPopulateDatabase()
		{
			string provider;
			string tmp;
			string loginName;
			string loginPwd;
			var configurator = CreateDbConfigurator(out provider, out tmp, out loginName, out loginPwd);

			//Setup master db schema
			var mainDbConnectionString = SessionProviderRegistry.GetMainDbProvider().ConnectionString;
			var now = DateTime.Now;
			var version = new Version(now.Year, now.Month, now.Day, 99);
			_MigrationService.MigrateUp(
				provider,
				mainDbConnectionString,
				ModuleDescriptors.Where(m => m.Alias == ModuleDescriptor.MODULE_CODE).Select(d => d.GetType().Assembly),
				version);
			
			foreach (var service in IocContainer.GetAllInstances<ITestDataService>())
			{
				foreach (var dbName in service.RequiredDatabases)
				{
					//Create each project test db
					configurator.CreateProjectDatabase(null, dbName, loginName, loginPwd);

					//and setup his schema
					//use mainDb cs instead of master because of created schema ownership must be to ago_user, not postgres
					var projConnectionString = configurator.MakeConnectionString(null, dbName, mainDbConnectionString);
					_MigrationService.MigrateUp(
						provider,
						projConnectionString,
						ModuleDescriptors.Select(d => d.GetType().Assembly),
						version);
				}
			}

			DoExecutePopulateDatabaseScript();

			SessionProviderRegistry.CloseCurrentSessions();
		}

		protected virtual void DoExecutePopulateDatabaseScript()
		{
			foreach (var service in IocContainer.GetAllInstances<ITestDataService>())
			{
				service.Populate();
				SessionProviderRegistry.CloseCurrentSessions();
			}
		}

		#endregion

		#region Helper methods

		protected void ExecuteNonQuery(string script, IDbConnection connection)
		{
			var scripts = new List<string>();
			using (var reader = new StringReader(script))
			{
				var line = reader.ReadLine();
				var currentBatch = new StringBuilder();
				while (line != null)
				{
					line = line.TrimSafe();
					if (!"GO".Equals(line, StringComparison.InvariantCultureIgnoreCase))
						currentBatch.AppendLine(line);
					else
					{
						if (currentBatch.Length > 0)
							scripts.Add(currentBatch.ToString());
						currentBatch = new StringBuilder();
					}

					line = reader.ReadLine();
				}

				if (currentBatch.Length > 0)
					scripts.Add(currentBatch.ToString());
			}

			foreach (var str in scripts)
			{
				var command = connection.CreateCommand();
				command.CommandText = str;
				command.CommandType = CommandType.Text;

				var rowsAffected = command.ExecuteNonQuery();
				if (rowsAffected >= 0)
					Console.WriteLine("Rows affected: {0}", rowsAffected);
			}

			Console.WriteLine("Batch complete ({0} scripts executed)", scripts.Count);
		}

		#endregion
	}
}