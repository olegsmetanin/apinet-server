using System;
using System.Collections.Generic;
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
			_AlternateHibernateConfigRegex = "^AlternateHibernate_(.*)";
			InitContainer();

			var databaseName = GetKeyValueProvider().Value("DatabaseName").TrimSafe();
			databaseName = databaseName.IsNullOrEmpty() ? "AGO_Docstore" : databaseName;
			
			ExecuteNonQuery(string.Format(@"
				IF EXISTS(SELECT name FROM sys.databases WHERE name = '{0}') BEGIN
					ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
					DROP DATABASE [{0}]
				END
				GO", databaseName));

			ExecuteNonQuery(string.Format(@"
				declare @Path nvarchar(500)
				set @Path = (select physical_name from sys.master_files WHERE name = 'master' AND type_desc ='ROWS')
				set @Path = (select SUBSTRING(@Path, 1, CHARINDEX('master.mdf', LOWER(@Path)) - 1))

				EXEC('CREATE DATABASE [{0}] ON  PRIMARY 
				( NAME = N''{0}'', FILENAME = N''' + @Path + '{0}.mdf'', SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
				 LOG ON 
				( NAME = N''{0}_log'', FILENAME = N''' + @Path + '{0}_log.ldf'', SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)')
				GO", databaseName));

			_MigrationService.MigrateUp(new Version(0, 9, 0, 0));
			_MigrationService.MigrateUp(new Version(1, 0, 0, 0));

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
			_Container.GetInstance<TestDataPopulationService>().PopulateCore();
			_Container.GetInstance<Home.TestDataPopulationService>().PopulateHome();

			_SessionProvider.CloseCurrentSession();
		}

		#region Container initialization

		protected override void Register()
		{
			RegisterEnvironment();
			RegisterPersistence();

			_Container.RegisterSingle<Core.TestDataPopulationService, Core.TestDataPopulationService>();
		}

		protected override void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);
		}

		protected override void DoMigrateUp()
		{
		}

		#endregion
	}
}
