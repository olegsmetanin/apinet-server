using FluentMigrator.VersionTableInfo;

namespace AGO.Core.Migrations
{
	public abstract class AbstractVersionTableMetaData : IVersionTableMetaData
	{
		public abstract string SchemaName { get; }

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
