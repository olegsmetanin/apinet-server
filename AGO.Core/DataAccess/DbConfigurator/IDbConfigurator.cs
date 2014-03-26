namespace AGO.Core.DataAccess.DbConfigurator
{
	/// <summary>
	/// Interface for direct db operations, that needed in our application.
	/// Hide implementation details for postgres and ms sql.
	/// At this point operations set contains:
	/// - master db creation
	/// - project db creation
	/// - connection string manipulation
	/// </summary>
	public interface IDbConfigurator
	{
		/// <summary>
		/// Master connection string to system database (postgre or master) with user priviledged
		/// for creating and configuring databases and logins/users
		/// </summary>
		string MasterConnectionString { get; }

		/// <summary>
		/// Make connection string for provided host and database, using template connection string
		/// </summary>
		/// <param name="host">Host or null (then host from template will be used)</param>
		/// <param name="database">Database name or null (then database from template will be used)</param>
		/// <param name="template">Template connection string or null (then <see cref="MasterConnectionString"/> will be used)</param>
		/// <returns>Builded connection string</returns>
		string MakeConnectionString(string host, string database, string template = null);

		/// <summary>
		/// Create master database of our application - contains projects list and linked stuff
		/// </summary>
		/// <param name="dbName">Name of master database</param>
		/// <param name="owner">Name of owner login (created if not exists)</param>
		/// <param name="ownerpwd">Password of owner login</param>
		void CreateMasterDatabase(string dbName, string owner, string ownerpwd);

		/// <summary>
		/// Create project database when project created - contains project data and related stuff
		/// </summary>
		/// <param name="host">Netbios name or ipaddress of database instance host</param>
		/// <param name="dbName">Name of project database</param>
		/// <param name="owner">Name of owner login (created if not exists)</param>
		/// <param name="ownerpwd">Password of owner login</param>
		void CreateProjectDatabase(string host, string dbName, string owner, string ownerpwd);
	}
}
