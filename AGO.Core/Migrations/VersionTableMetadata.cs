using FluentMigrator.VersionTableInfo;

namespace AGO.Core.Migrations
{
	[VersionTableMetaData]
	public class VersionTableMetaData : IVersionTableMetaData
	{
		public string SchemaName
		{
			get { return string.Empty; }
		}

		public string TableName
		{
			get { return GetType().Assembly.GetName().Name.Replace('.', '_') + "_VersionInfo"; }
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
