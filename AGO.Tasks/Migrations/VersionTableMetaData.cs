using AGO.Core.Migrations;
using FluentMigrator.VersionTableInfo;

namespace AGO.Tasks.Migrations
{
    [VersionTableMetaData]
    public class VersionTableMetaData: AbstractVersionTableMetaData
    {
        public override string SchemaName
        {
            get { return TasksBootstrapMigration.MODULE_SCHEMA; }
        }
    }
}