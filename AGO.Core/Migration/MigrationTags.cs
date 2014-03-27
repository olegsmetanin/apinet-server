namespace AGO.Core.Migration
{
	/// <summary>
	/// Tags, used for taggin migrations
	/// </summary>
	public static class MigrationTags
	{
		/// <summary>
		/// Migrations, that exists in cental main database
		/// </summary>
		public const string MasterDb = "MasterDbMigration";

		/// <summary>
		/// Migrations, that exists in project database
		/// </summary>
		public const string ProjectDb = "ProjectDbMigration";
	}
}
