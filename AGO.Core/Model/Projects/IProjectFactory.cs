namespace AGO.Core.Model.Projects
{
	/// <summary>
	/// Handle newly created projects and apply project/module type specific logic
	/// </summary>
	public interface IProjectFactory
	{
		/// <summary>
		/// Determine, if this factory applicable to new project
		/// </summary>
		/// <param name="project">New project</param>
		bool Accept(ProjectModel project);

		/// <summary>
		/// Apply specific logic to created project
		/// </summary>
		/// <param name="project">New project</param>
		void Handle(ProjectModel project);
	}
}