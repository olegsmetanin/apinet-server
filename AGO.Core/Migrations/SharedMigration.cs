using System;
using AGO.Core.Migration;
using AGO.Core.Model.Dictionary;
using FluentMigrator;

namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 01), Tags(MigrationTags.MasterDb, MigrationTags.ProjectDb)]
	public class SharedMigration: FluentMigrator.Migration
	{
		public override void Up()
		{
			var provider = ApplicationContext as string;
			// ReSharper disable once InconsistentNaming
			var use_citext = provider != null && provider.StartsWith("postgres", StringComparison.InvariantCultureIgnoreCase);

			Create.CoreModelTable<TagModel>()
				.WithValueColumn<TagModel>(m => m.OwnerId)
				.WithValueColumn<TagModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<TagModel>(m => m.Name, use_citext)
				.WithValueColumn<TagModel>(m => m.FullName, use_citext)
				.WithRefColumn<TagModel>(m => m.Parent);
		}

		public override void Down()
		{
			Delete.ModelTable<TagModel>();
		}
	}
}
