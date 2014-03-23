using System;
using System.Collections.Generic;
using System.Threading;
using AGO.Core.AutoMapping;
using AGO.Core.Config;
using AGO.Core.Model.Projects;

namespace AGO.Core
{
	public class SessionProviderRegistry: AbstractService, ISessionProviderRegistry
	{
		private readonly IEnvironmentService environmentService;
		private IKeyValueProvider mainDbConfigProvider;
		private ISessionProvider mainDbProvider;
		private readonly IDictionary<string, ISessionProvider> projectDbProviders;
		private readonly ReaderWriterLockSlim rwlock;

		public SessionProviderRegistry(IEnvironmentService environmentService)
		{
			if (environmentService == null)
				throw new ArgumentNullException("environmentService");

			this.environmentService = environmentService;
			projectDbProviders = new Dictionary<string, ISessionProvider>();
			rwlock = new ReaderWriterLockSlim();
		}

		internal void Initialize(IKeyValueProvider mainDbConfig)
		{
			mainDbConfigProvider = mainDbConfig;
			var provider = new AutoMappedSessionFactoryBuilder(environmentService);
			new KeyValueConfigProvider(mainDbConfigProvider).ApplyTo(provider);
			provider.Initialize();

			mainDbProvider = provider;
		}

		public ISessionProvider GetMainDbProvider()
		{
			return mainDbProvider;
		}

		public ISessionProvider GetProjectProvider(string project)
		{
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentNullException("project");

			rwlock.EnterUpgradeableReadLock();
			try
			{
				if (projectDbProviders.ContainsKey(project))
					return projectDbProviders[project];

				var pm = mainDbProvider.CurrentSession
					.QueryOver<ProjectModel>()
					.Where(m => m.ProjectCode == project)
					.SingleOrDefault();
					if (pm == null)
						throw new NoSuchProjectException();

				rwlock.EnterWriteLock();
				try
				{
					var config = new ProjectConfig {Original = mainDbConfigProvider, ConnectionString = pm.ConnectionString};
					var provider = new AutoMappedSessionFactoryBuilder(environmentService);
					new KeyValueConfigProvider(config).ApplyTo(provider);
					provider.Initialize();

					projectDbProviders[project] = provider;
					return provider;
				}
				finally
				{
					rwlock.ExitWriteLock();
				}
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}

		public void CloseCurrentSessions(bool forceRollback = false)
		{
			mainDbProvider.CloseCurrentSession(forceRollback);
			foreach (var projsf in projectDbProviders.Values)
			{
				projsf.CloseCurrentSession(forceRollback);
			}
		}

		public void DropCachedProviders()
		{
			rwlock.EnterWriteLock();
			try
			{
				foreach (var projsf in projectDbProviders.Values)
				{
					projsf.CloseCurrentSession(true);
				}
				projectDbProviders.Clear();
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}

		private class ProjectConfig : IKeyValueProvider
		{
			public IKeyValueProvider Original;
			public string ConnectionString;

			public IEnumerable<string> Keys 
			{
				get { return Original.Keys; }
			}

			public string RealKey(string key)
			{
				return Original.RealKey(key);
			}

			public string Value(string key)
			{
				if ("Hibernate_connection.connection_string".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					return ConnectionString;
				}

				return Original.Value(key);
			}
		}
	}
}
