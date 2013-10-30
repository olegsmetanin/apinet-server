using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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

		protected IList<Type> _TestDataServices = new List<Type>();
		public IList<Type> TestDataServices { get { return _TestDataServices; } }
		
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

		protected virtual void DoCreateDatabase()
		{
			var config = new KeyValueConfigurableDictionary();
			new KeyValueConfigProvider(new RegexKeyValueProvider("^Persistence_(.*)", KeyValueProvider)).ApplyTo(config);

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

			var masterConnection = new SqlConnection(masterConnectionStr);
			try
			{
				masterConnection.Open();
				DoExecuteCreateDatabaseScript(masterConnection, databaseName, loginName, loginPwd);
			}
			finally
			{
				masterConnection.Close();
			}
		}

		protected virtual void DoExecuteCreateDatabaseScript(
			IDbConnection masterConnection,
			string databaseName,
			string loginName,
			string loginPwd)
		{
			ExecuteNonQuery(string.Format(@"
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
				end			
				go", databaseName, loginName, loginPwd), masterConnection);
		}

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