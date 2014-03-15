using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AGO.Core.AutoMapping;
using AGO.Core.Config;
using AGO.Core.DataAccess;
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

		[Obsolete("Replace with SessionProviderRegistry when it will be implemented")]
		protected ISessionProvider _SessionProvider;
		[Obsolete("Replace with SessionProviderRegistry when it will be implemented")]
		public ISessionProvider SessionProvider { get { return _SessionProvider; } }

		public ISessionProviderRegistry SessionProviderRegistry { get; private set; }

		protected IFilteringService _FilteringService;
		public IFilteringService FilteringService { get { return _FilteringService; } }

		protected IFilteringDao _FilteringDao;
		public IFilteringDao FilteringDao { get { return _FilteringDao; } }

		protected ICrudDao _CrudDao;
		public ICrudDao CrudDao { get { return _CrudDao; } }
		public DaoFactory DaoFactory { get; private set; }

		protected IMigrationService _MigrationService;
		public IMigrationService MigrationService { get { return _MigrationService; } }

		protected IModelProcessingService _ModelProcessingService;
		public IModelProcessingService ModelProcessingService { get { return _ModelProcessingService; } }

		protected IList<Type> _TestDataServices = new List<Type>();
		public IList<Type> TestDataServices { get { return _TestDataServices; } }

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
			IocContainer.RegisterSingle<ISessionProvider, AutoMappedSessionFactoryBuilder>();
			IocContainer.RegisterInitializer<AutoMappedSessionFactoryBuilder>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<ISessionProviderRegistry, SessionProviderRegistry>();
			IocContainer.RegisterInitializer<SessionProviderRegistry>(service =>
				service.Initialize(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)));

			IocContainer.RegisterSingle<IFilteringService, FilteringService>();
			IocContainer.RegisterInitializer<FilteringService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Filtering_(.*)", KeyValueProvider)).ApplyTo(service));

			IocContainer.RegisterSingle<DaoFactory>();

			IocContainer.RegisterSingle<CrudDao, CrudDao>();

			IocContainer.Register<ICrudDao>(IocContainer.GetInstance<CrudDao>);
			IocContainer.Register<IFilteringDao>(IocContainer.GetInstance<CrudDao>);

			IocContainer.RegisterSingle<IMigrationService, MigrationService>();
			IocContainer.RegisterInitializer<MigrationService>(service =>
				new KeyValueConfigProvider(new RegexKeyValueProvider("^Hibernate_(.*)", KeyValueProvider)).ApplyTo(service));

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

		protected virtual Type NotificationServiceType { get { return typeof (NotificationService); } }

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
				var sp = IocContainer.GetInstance<ISessionProvider>();
				return new PostgreSqlWorkQueue(sp.ConnectionString, schema);
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
			_SessionProvider = IocContainer.GetInstance<ISessionProvider>();
			SessionProviderRegistry = IocContainer.GetInstance<ISessionProviderRegistry>();
			_FilteringService = IocContainer.GetInstance<IFilteringService>();
			DaoFactory = IocContainer.GetInstance<DaoFactory>();
			_FilteringDao = IocContainer.GetInstance<IFilteringDao>();
			_CrudDao = IocContainer.GetInstance<ICrudDao>();
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

		protected virtual void DoCreateDatabase()
		{
			var config = new KeyValueConfigurableDictionary();
			new KeyValueConfigProvider(new RegexKeyValueProvider("^Persistence_(.*)", KeyValueProvider)).ApplyTo(config);

			ProviderName = config.GetConfigProperty("ProviderName").TrimSafe();
			if (ProviderName.IsNullOrEmpty())
				throw new Exception("ProviderName is empty");

			var masterConnectionStr = config.GetConfigProperty("MasterConnectionString").TrimSafe();
			if (masterConnectionStr.IsNullOrEmpty())
				throw new Exception("MasterConnectionString is empty");

			var databaseName = config.GetConfigProperty("DatabaseName").TrimSafe();
			if (databaseName.IsNullOrEmpty())
				throw new Exception("DatabaseName is empty");

			var loginName = config.GetConfigProperty("LoginName").TrimSafe();
			if (loginName.IsNullOrEmpty())
				throw new Exception("LoginName is empty");

			var loginPwd = config.GetConfigProperty("LoginPwd").TrimSafe();
			if (loginPwd.IsNullOrEmpty())
				throw new Exception("LoginPwd is empty");

			var connectionFactory = DbProviderFactories.GetFactory(ProviderName);
			using (var masterConnection = connectionFactory.CreateConnection())
			{
				Debug.Assert(masterConnection != null, "connectionFactory does not create DbConnection instance");

				masterConnection.ConnectionString = masterConnectionStr;
				masterConnection.Open();
				DoExecuteCreateDatabaseScript(masterConnection, ProviderName, databaseName, loginName, loginPwd);
				masterConnection.Close();
			}
		}

		protected string ProviderName { get; private set; }

		protected virtual void DoExecuteCreateDatabaseScript(
			IDbConnection masterConnection,
			string provider,
			string databaseName,
			string loginName,
			string loginPwd)
		{
			var sql = CreateDbScripts[provider];
			ExecuteNonQuery(string.Format(sql, databaseName, loginName, loginPwd), masterConnection);
		}

		private static readonly Dictionary<string, string> CreateDbScripts = new Dictionary<string, string>
		{
			{"PostgreSQL", @"
				select pg_terminate_backend(pg_stat_activity.pid)
				from pg_stat_activity
				where datname = '{0}';
				go
				drop database if exists {0};
				go
				drop role if exists {1};
				go
				create database {0};
				go
				create role {1} login password '{2}';
				go
				alter database {0} owner to {1}"},

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
			DoMigrateUp();

			DoExecutePopulateDatabaseScript();

			_SessionProvider.CloseCurrentSession();
		}

		protected virtual void DoMigrateUp()
		{
			var now = DateTime.Now;
			_MigrationService.MigrateUp(new Version(now.Year, now.Month, now.Day, 99));
		}

		protected virtual void DoExecutePopulateDatabaseScript()
		{
			foreach (var service in IocContainer.GetAllInstances<ITestDataService>())
			{
				service.Populate();
				_SessionProvider.FlushCurrentSession();
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