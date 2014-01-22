using System;
using System.Linq;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.VersionTableInfo;
using AGO.Core.AutoMapping;
using FluentMigrator.Runner.Processors.Postgres;

namespace AGO.Core.Migration
{
	public class MigrationService : AutoMappedSessionFactoryBuilder, IMigrationService
	{
		#region Properties, fields, constructors

		public MigrationService(IEnvironmentService environmentService)
			:base(environmentService)
		{
		}

		#endregion

		#region Interfaces implementation

		public void MigrateUp(Version upToVersion = null, bool previewOnly = false)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			foreach (var assembly in AutoMappingAssemblies)
			{
				var versionTableType =
					assembly.GetTypes().Aggregate<Type, Type>(null, (result, current) => typeof(IVersionTableMetaData).IsAssignableFrom(current) ? current : result);
				if (versionTableType == null)
					continue;
				var runner = GetMigrationRunner(assembly, previewOnly);
				var currentUpToVersion = upToVersion ?? assembly.GetName().Version;
				var version = GetMigrationVersion(currentUpToVersion);

				Log.Info(string.Format("Updating database structure for assembly {4} to version {0}.{1}.{2}.{3}",
					currentUpToVersion.Major, currentUpToVersion.Minor, currentUpToVersion.Build, currentUpToVersion.Revision, assembly.GetName().Name));
				runner.MigrateUp(version, true);
			}
		}

		public void MigrateDown(Version downToVersion = null, bool previewOnly = false)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			foreach (var assembly in AutoMappingAssemblies)
			{
				var versionTableType =
					assembly.GetTypes().Aggregate<Type, Type>(null, (result, current) => typeof(IVersionTableMetaData).IsAssignableFrom(current) ? current : result);
				if (versionTableType == null)
					continue;
				var runner = GetMigrationRunner(assembly, previewOnly);
				var av = assembly.GetName().Version;
				var currentDownToVersion = downToVersion ?? new Version(av.Major, av.Minor, av.Build - 1, av.Revision);
				var version = GetMigrationVersion(currentDownToVersion);
				Log.Info(string.Format("Reverting database structure for assembly {4} to version {0}.{1}.{2}.{3}",
					currentDownToVersion.Major, currentDownToVersion.Minor, currentDownToVersion.Build, currentDownToVersion.Revision, assembly.GetName().Name));
				runner.MigrateDown(version, true);
			}
		} 

		#endregion

		#region Template methods

		protected override void DoFinalizeConfig()
		{
			if (Dialect == null)
				throw new ArgumentException("Dialect");
		}

		protected override void DoBuildSessionFactory()
		{
		}

		#endregion

		#region Helper methods

		protected long GetMigrationVersion(Version version)
		{
			//return version.Major * 10000 + version.Minor * 1000 + version.Build * 100 + version.Revision;
            return MigrationVersionAttribute.CalculateVersion(version.Major, version.Minor, version.Build, version.Revision);
		}

		protected MigrationRunner GetMigrationRunner(Assembly assembly, bool previewOnly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");
			var announcer = new TextWriterAnnouncer(Console.Out) { ShowSql = true };
			var runnerContext = new RunnerContext(announcer);
			var processorOptions = new ProcessorOptions { Timeout = 60, PreviewOnly = previewOnly };
			var pgsqlFactory = new PostgresProcessorFactory();
			var sqliteFactory = new SqliteProcessorFactory();
			var sql2008Factory = new SqlServer2008ProcessorFactory();
			var sql2005Factory = new SqlServer2005ProcessorFactory();
			var sql2000Factory = new SqlServer2000ProcessorFactory();
			IMigrationProcessor migrationProcessor = null;

			if (Dialect.Name.StartsWith("SQLite", StringComparison.InvariantCultureIgnoreCase))
			{
				migrationProcessor = sqliteFactory.Create(ConnectionString, announcer, processorOptions);
			}
			else if (Dialect.Name.StartsWith("PostgreSQL", StringComparison.InvariantCultureIgnoreCase))
			{
				migrationProcessor = pgsqlFactory.Create(ConnectionString, announcer, processorOptions);
			}
			else if (Dialect.Name.StartsWith("MsSql", StringComparison.InvariantCultureIgnoreCase))
			{
				if (Dialect.Name.Contains("2008"))
					migrationProcessor = sql2008Factory.Create(ConnectionString, announcer, processorOptions);
				else if (Dialect.Name.Contains("2005"))
					migrationProcessor = sql2005Factory.Create(ConnectionString, announcer, processorOptions);
				else if (Dialect.Name.Contains("2000"))
					migrationProcessor = sql2000Factory.Create(ConnectionString, announcer, processorOptions);
			}

			if (migrationProcessor == null)
				throw new Exception(string.Format("No supported migration processor found for dialect: {0}", Dialect));

			var migrationRunner = new MigrationRunner(assembly, runnerContext, migrationProcessor);
			migrationRunner.VersionLoader = new VersionLoader(migrationRunner, assembly, new MigrationConventions());
			return migrationRunner;
		}

		#endregion
	}
}
