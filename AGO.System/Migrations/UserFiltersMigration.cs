using AGO.Core.Migration;
using AGO.System.Model;
using FluentMigrator;

namespace AGO.System.Migrations
{
	[MigrationVersion(2013, 09, 02, 01)]
	public class UserFiltersMigration : Migration
	{
		internal const string MODULE_SCHEMA = "System";

		public override void Up()
		{
			Create.DocstoreModelTable<UserFilterModel>()
				.WithValueColumn<UserFilterModel>(m => m.Name)
				.WithValueColumn<UserFilterModel>(m => m.GroupName)
				.WithValueColumn<UserFilterModel>(m => m.Filter)
				.WithRefColumn<UserFilterModel>(m => m.User);
		}

		public override void Down()
		{
		}
	}
}
