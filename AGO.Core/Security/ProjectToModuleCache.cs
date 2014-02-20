using System;
using System.Collections.Concurrent;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Core.Security
{
	public class ProjectToModuleCache
	{
		private readonly string module;
		private readonly ConcurrentDictionary<string, string> cache;

		public ProjectToModuleCache(string module)
		{
			if (module.IsNullOrWhiteSpace())
				throw new ArgumentNullException("module");

			this.module = module;
			cache = new ConcurrentDictionary<string, string>();
		}

		protected ProjectModel CodeToProject(string project, ISession session)
		{
			var p = session.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == project)
				.SingleOrDefault();
			if (p == null)
				throw new NoSuchProjectException();
			return p;
		}

		
		/// <summary>
		/// If two project from other modules states in one db, we need use only appropriate provider
		/// for project module and don't touch logic of projects in other module. So, added this check
		/// </summary>
		public bool IsProjectInHandledModule(string project, ISession session)
		{
			if (!cache.ContainsKey(project))
			{
				cache[project] = CodeToProject(project, session).Type.Module;
			}
			return cache[project].Equals(module, StringComparison.InvariantCultureIgnoreCase);
		} 
	}
}