using FluentMigrator.VersionTableInfo;

namespace AGO.Tasks.Migrations
{
    [VersionTableMetaData]
    public class VersionTableMetaData: IVersionTableMetaData
    {
        public string SchemaName
        {
            get { return TasksBootstrapMigration.MODULE_SCHEMA; }
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
            get { return "IX_VersionInfo_Version"; }
        }
    }
}