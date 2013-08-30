using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using AGO.Core.Migration;
using FluentMigrator;

namespace AGO.Home.Migrations
{
	[Migration(10000)]
	public class ProjectsMigration : Migration
	{
		public override void Up()
		{
			Create.SecureModelTable<ProjectStatusModel>()
				.WithValueColumn<ProjectStatusModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectStatusModel>(m => m.Name)
				.WithValueColumn<ProjectStatusModel>(m => m.Description);

			Create.SecureModelTable<ProjectModel>()
				.WithValueColumn<ProjectModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectModel>(m => m.Name)
				.WithValueColumn<ProjectModel>(m => m.Description)
				.WithValueColumn<ProjectModel>(m => m.IsArchive)
				.WithValueColumn<ProjectModel>(m => m.EventsHorizon)
				.WithValueColumn<ProjectModel>(m => m.FileSystemPath)
				.WithRefColumn<ProjectModel>(m => m.Status);

			Create.SecureModelTable<ProjectStatusHistoryModel>()
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.StartDate)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.EndDate)
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Project)
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Status);

			Create.DocstoreModelTable<ProjectParticipantModel>()
				.WithValueColumn<ProjectParticipantModel>(m => m.GroupName)
				.WithValueColumn<ProjectParticipantModel>(m => m.IsDefaultGroup)
				.WithRefColumn<ProjectParticipantModel>(m => m.Project)
				.WithRefColumn<ProjectParticipantModel>(m => m.User);

		}

		public override void Down()
		{
		}
	}
}
