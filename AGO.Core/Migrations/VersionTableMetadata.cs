using FluentMigrator.VersionTableInfo;

namespace AGO.Core.Migrations
{
	[VersionTableMetaData]
	public class VersionTableMetaData : IVersionTableMetaData
	{
		public string SchemaName
		{
			get { return CoreMigration.MODULE_SCHEMA; }
		}

		public string TableName
		{
			get { return "VersionInfo"; }
		}

		public string ColumnName
		{
			get { return "Version"; }
		}

		public string UniqueIndexName
		{
			get { return "IX_" + GetType().Assembly.GetName().Name.Replace('.', '_') + "_Version"; }
		}
	}
}
