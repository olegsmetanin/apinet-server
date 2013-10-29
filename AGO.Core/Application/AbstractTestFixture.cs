using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace AGO.Core.Application
{
	public abstract class AbstractTestFixture : AbstractControllersApplication
	{
		#region Template methods

		protected virtual void DoCreateDatabase()
		{
			var masterConnectionStr = KeyValueProvider.Value("MasterConnectionString").TrimSafe();
			if (masterConnectionStr.IsNullOrWhiteSpace())
				throw new Exception("masterConnectionStr is empty");

			var databaseName = KeyValueProvider.Value("DatabaseName").TrimSafe();
			if (databaseName.IsNullOrWhiteSpace())
				throw new Exception("databaseName is empty");

			var loginName = KeyValueProvider.Value("LoginName").TrimSafe();
			if (loginName.IsNullOrWhiteSpace())
				throw new Exception("loginName is empty");

			var loginPwd = KeyValueProvider.Value("LoginPwd").TrimSafe();
			if (loginPwd.IsNullOrWhiteSpace())
				throw new Exception("loginPwd is empty");

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
			foreach (var service in IocContainer.GetAllInstances<IModuleTestDataService>())
			{
				service.Populate();
				_SessionProvider.FlushCurrentSession();
			}
		}

		protected override void DoRegisterCoreServices()
		{
			base.DoRegisterCoreServices();

			IocContainer.RegisterAll<IModuleTestDataService>(ModuleTestDataServices);
		}

		protected virtual IEnumerable<Type> ModuleTestDataServices
		{
			get { return new[] { typeof(ModuleTestDataService) }; }
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