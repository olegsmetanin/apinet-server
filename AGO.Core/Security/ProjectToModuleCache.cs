using System;
using System.Collections.Concurrent;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Core.Security
{
	public class ProjectToModuleCache
	{
		private readonly string module;
		private readonly ISessionFactory mainDbSessionFactory;
		private readonly ConcurrentDictionary<string, string> cache;

		public ProjectToModuleCache(string module, ISessionFactory mainFactory)
		{
			if (module.IsNullOrWhiteSpace())
				throw new ArgumentNullException("module");
			if (mainFactory == null)
				throw new ArgumentNullException("mainFactory");

			this.module = module;
			mainDbSessionFactory = mainFactory;
			cache = new ConcurrentDictionary<string, string>();
		}

		protected ProjectModel CodeToProject(string project)
		{
			var s = mainDbSessionFactory.OpenStatelessSession();
			try
			{
				var p = s.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == project)
				.SingleOrDefault();
				if (p == null)
					throw new NoSuchProjectException();
				return p;
			}
			finally
			{
				s.Close();
			}
		}

		
		/// <summary>
		/// If two project from other modules states in one db, we need use only appropriate provider
		/// for project module and don't touch logic of projects in other module. So, added this check
		/// </summary>
		public bool IsProjectInHandledModule(string project)
		{
			if (!cache.ContainsKey(project))
			{
				cache[project] = CodeToProject(project).Type.Module;
			}
			return cache[project].Equals(module, StringComparison.InvariantCultureIgnoreCase);
		} 
	}
}