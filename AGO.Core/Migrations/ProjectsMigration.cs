using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Documents;
using AGO.Core.Model.Projects;
using AGO.Hibernate.Migration;
using FluentMigrator;

namespace AGO.Core.Migrations
{
	[Migration(10200)]
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

			Create.SecureModelTable<DocumentStatusHistoryModel>()
				.WithValueColumn<DocumentStatusHistoryModel>(m => m.StartDate)
				.WithValueColumn<DocumentStatusHistoryModel>(m => m.EndDate)
				.WithRefColumn<DocumentStatusHistoryModel>(m => m.Document)
				.WithRefColumn<DocumentStatusHistoryModel>(m => m.Status);

		}

		public override void Down()
		{
			Delete.ModelTable<DocumentStatusHistoryModel>();

			Delete.ModelTable<ProjectParticipantModel>();
			Delete.ModelTable<ProjectStatusHistoryModel>();
			Delete.ModelTable<ProjectModel>();
			Delete.ModelTable<ProjectStatusModel>();
		}
	}
}
