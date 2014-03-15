namespace AGO.Core
{
	/// <summary>
	/// Registry for <see cref="ISessionProvider"/> instances, one (or pool) for cental database 
	/// with users, projects and settings and on (or pool) for each project (that may resides in separate server and(or) db).
	/// Implementation must be threadsafe.
	/// </summary>
	public interface ISessionProviderRegistry
	{
		/// <summary>
		/// Return session provider for access to central data store
		/// </summary>
		ISessionProvider GetMainDbProvider();

		/// <summary>
		/// Return session provider for access to project data store
		/// </summary>
		/// <param name="project">Project code</param>
		ISessionProvider GetProjectProvider(string project);

		/// <summary>
		/// Close all binded to context session. Optionally rollback changes
		/// </summary>
		/// <param name="forceRollback">Rollback changes in open sessions</param>
		void CloseCurrentSessions(bool forceRollback = false);

		/// <summary>
		/// Clear project data store providers cache
		/// </summary>
		void DropCachedProviders();
	}
}
