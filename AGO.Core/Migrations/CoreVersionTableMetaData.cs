using FluentMigrator.VersionTableInfo;

namespace AGO.Core.Migrations
{
	[VersionTableMetaData]
	public class CoreVersionTableMetaData : AbstractVersionTableMetaData
	{
		public override string SchemaName
		{
			get { return CoreMigration.MODULE_SCHEMA; }
		}
	}
}
