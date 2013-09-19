using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AGO.Core;
using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.WebApi.Tests
{
	[TestFixture]
	public class TestDataPopulation : AbstractApplication
	{
		[Test]
		public void CreateAndPopulateDatabase()
		{
			var databaseName = GetKeyValueProvider().Value("DatabaseName").TrimSafe();
			Assert.IsNotEmpty(databaseName);

			var masterConnectionStr = GetKeyValueProvider().Value("MasterConnectionString").TrimSafe();
			Assert.IsNotEmpty(masterConnectionStr);

			var masterConnection = new SqlConnection(masterConnectionStr);
			try
			{
				masterConnection.Open();

				ExecuteNonQuery(string.Format(@"
					IF EXISTS(SELECT name FROM sys.databases WHERE name = '{0}') BEGIN
						ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
						DROP DATABASE [{0}]
					END
					GO", databaseName), masterConnection);

				ExecuteNonQuery(string.Format(@"
					declare @Path nvarchar(500)
					set @Path = (select physical_name from sys.master_files WHERE name = 'master' AND type_desc ='ROWS')
					set @Path = (select SUBSTRING(@Path, 1, CHARINDEX('master.mdf', LOWER(@Path)) - 1))

					EXEC('CREATE DATABASE [{0}] ON  PRIMARY 
					( NAME = N''{0}'', FILENAME = N''' + @Path + '{0}.mdf'', SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
					 LOG ON 
					( NAME = N''{0}_log'', FILENAME = N''' + @Path + '{0}_log.ldf'', SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)')
					GO", databaseName), masterConnection);
			}
			finally
			{
				masterConnection.Close();
			}

			InitContainer();
			DoPopulateDatabase();
		}

		[Test]
		public void PopulateDatabase()
		{
			InitContainer();
			DoPopulateDatabase();
		}

		protected void DoPopulateDatabase()
		{
		    var now = DateTime.Now;
		    _MigrationService.MigrateUp(new Version(now.Year, now.Month, now.Day, 99));

			_Container.GetInstance<TestDataPopulationService>().PopulateCore();
			_Container.GetInstance<Home.TestDataPopulationService>().PopulateHome();

			_SessionProvider.CloseCurrentSession();
		}

		#region Container initialization

		protected override void Register()
		{
			RegisterEnvironment();
			RegisterPersistence();
		}

		protected override void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);
		}

		#endregion
	}
}
