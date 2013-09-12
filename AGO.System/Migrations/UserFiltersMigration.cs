using AGO.Core.Migration;
using AGO.System.Model;
using FluentMigrator;

namespace AGO.System.Migrations
{
	[Migration(10000)]
	public class UserFiltersMigration : Migration
	{
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
