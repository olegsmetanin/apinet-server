using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Core.Migration;
using FluentMigrator;

namespace AGO.Home.Migrations
{
	[MigrationVersion(2013, 09, 05, 01)]
	public class ProjectsMigration : Migration
	{
		internal const string MODULE_SCHEMA = "Home";

		public override void Up()
		{
			Create.SecureModelTable<ProjectTypeModel>()
				.WithValueColumn<ProjectTypeModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectTypeModel>(m => m.Name)
				.WithValueColumn<ProjectTypeModel>(m => m.Description)
				.WithValueColumn<ProjectTypeModel>(m => m.Module);

			Create.SecureModelTable<ProjectModel>()
				.WithValueColumn<ProjectModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectModel>(m => m.Name)
				.WithValueColumn<ProjectModel>(m => m.Description)
				.WithRefColumn<ProjectModel>(m => m.Type)
				.WithValueColumn<ProjectModel>(m => m.Status);

			Create.SecureModelTable<ProjectStatusHistoryModel>()
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.StartDate)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.EndDate)
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Project)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Status);

			Create.CoreModelTable<ProjectParticipantModel>()
				.WithValueColumn<ProjectParticipantModel>(m => m.GroupName)
				.WithValueColumn<ProjectParticipantModel>(m => m.IsDefaultGroup)
				.WithRefColumn<ProjectParticipantModel>(m => m.Project)
				.WithRefColumn<ProjectParticipantModel>(m => m.User);

			Create.SecureModelTable<ProjectToTagModel>()
				.WithRefColumn<ProjectToTagModel>(m => m.Project)
				.WithRefColumn<ProjectToTagModel>(m => m.Tag);

		}

		public override void Down()
		{
		}
	}
}
