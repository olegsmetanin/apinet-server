using System;
using AGO.Core;
using AGO.Core.Model.Projects;

namespace AGO.Tasks
{
	public sealed class TasksProjectFactory: IProjectFactory
	{
		private readonly ISessionProviderRegistry registry;

		public TasksProjectFactory(ISessionProviderRegistry providerRegistry)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			registry = providerRegistry;
		}

		public bool Accept(ProjectModel project)
		{
			return project != null && project.Type != null && project.Type.Module == ModuleDescriptor.MODULE_CODE;
		}

		public void Handle(ProjectModel project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			var session = registry.GetProjectProvider(project.ProjectCode).CurrentSession;
			new TasksReports().PopulateReports(session, project.ProjectCode);
			session.Flush();
		}
	}
}