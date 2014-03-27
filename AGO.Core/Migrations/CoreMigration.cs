using System;
using AGO.Core.Model.Configuration;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Migration;
using FluentMigrator;


namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 02), Tags(MigrationTags.MasterDb)]
	public class CoreMigration : FluentMigrator.Migration
	{
		internal const string MODULE_SCHEMA = "\"Core\"";

		public override void Up()
		{
			var provider = ApplicationContext as string;
// ReSharper disable once InconsistentNaming
			var use_citext = provider != null && provider.StartsWith("postgres", StringComparison.InvariantCultureIgnoreCase);

			Create.CoreModelTable<DbInstanceModel>()
				.WithValueColumn<DbInstanceModel>(m => m.Name, use_citext)
				.WithValueColumn<DbInstanceModel>(m => m.Server)
				.WithValueColumn<DbInstanceModel>(m => m.Provider);

			Create.CoreModelTable<UserModel>()
				.WithValueColumn<UserModel>(m => m.Email, use_citext)
				.WithValueColumn<UserModel>(m => m.Active)
				.WithValueColumn<UserModel>(m => m.FirstName, use_citext)
				.WithValueColumn<UserModel>(m => m.LastName, use_citext)
				.WithValueColumn<UserModel>(m => m.FullName, use_citext)
				.WithValueColumn<UserModel>(m => m.SystemRole)
				.WithValueColumn<UserModel>(m => m.AvatarUrl)
				.WithValueColumn<UserModel>(m => m.OAuthProvider)
				.WithValueColumn<UserModel>(m => m.OAuthUserId);

			Create.CoreModelTable<OAuthDataModel>()
				.WithValueColumn<OAuthDataModel>(m => m.RedirectUrl)
				.WithValueColumn<TwitterOAuthDataModel>(m => m.Token)
				.WithValueColumn<TwitterOAuthDataModel>(m => m.TokenSecret);

			Create.CoreModelTable<ProjectTypeModel>()
				.WithValueColumn<ProjectTypeModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ProjectTypeModel>(m => m.Name, use_citext)
				.WithValueColumn<ProjectTypeModel>(m => m.Description, use_citext)
				.WithValueColumn<ProjectTypeModel>(m => m.Module, use_citext);

			Create.CoreModelTable<ProjectModel>()
				.WithValueColumn<ProjectModel>(m => m.ProjectCode, use_citext).Unique("IX_ProjectModel_ProjectCode")
				.WithValueColumn<ProjectModel>(m => m.Name, use_citext)
				.WithValueColumn<ProjectModel>(m => m.Description, use_citext)
				.WithRefColumn<ProjectModel>(m => m.Type)
				.WithValueColumn<ProjectModel>(m => m.VisibleForAll)
				.WithValueColumn<ProjectModel>(m => m.Status)
				.WithValueColumn<ProjectModel>(m => m.ConnectionString);

			Create.CoreModelTable<ProjectMembershipModel>()
				.WithRefColumn<ProjectMembershipModel>(pm => pm.Project)
				.WithRefColumn<ProjectMembershipModel>(m => m.User);

			Create.CoreModelTable<ProjectStatusHistoryModel>()
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Creator)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Start)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Finish)
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Project)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Status);

			Create.CoreModelTable<ProjectToTagModel>()
				.WithRefColumn<ProjectToTagModel>(m => m.Project)
				.WithRefColumn<ProjectToTagModel>(m => m.Tag);

			Create.CoreModelTable<ReportArchiveRecordModel>()
				.WithValueColumn<ReportArchiveRecordModel>(m => m.ReportTaskId)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.ProjectName, use_citext)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.ProjectType, use_citext)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.Name, use_citext)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.SettingsName, use_citext)
				.WithValueColumn<ReportArchiveRecordModel>(m => m.UserId);
				
			//Hash token:login, that will be used in socket.io for sending messages
			//to particulat user
			Create.Table("TokenToLogin").InSchema(MODULE_SCHEMA)
				.WithColumn("Token").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("Login").AsString(64).NotNullable()
				.WithColumn("CreatedAt").AsDateTime().NotNullable();

			//WorkQueue, will be used later as source for all async worker (now only for reporting service)
			Create.Table("WorkQueue").InSchema(MODULE_SCHEMA)
				.WithColumn("TaskType").AsString(128).NotNullable()
				.WithColumn("TaskId").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("Project").AsString(ProjectModel.PROJECT_CODE_SIZE).NotNullable()
				.WithColumn("User").AsString(128).NotNullable()
				.WithColumn("CreateDate").AsDateTime().NotNullable()
				.WithColumn("PriorityType").AsInt32().NotNullable()
				.WithColumn("UserPriority").AsInt32().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("WorkQueue").InSchema(MODULE_SCHEMA);
			Delete.Table("TokenToLogin").InSchema(MODULE_SCHEMA);
			Delete.ModelTable<ReportArchiveRecordModel>();
			Delete.ModelTable<ProjectToTagModel>();
			Delete.ModelTable<ProjectStatusHistoryModel>();
			Delete.ModelTable<ProjectMembershipModel>();
			Delete.ModelTable<ProjectModel>();
			Delete.ModelTable<ProjectTypeModel>();
			Delete.ModelTable<OAuthDataModel>();
			Delete.ModelTable<UserModel>();
			Delete.ModelTable<DbInstanceModel>();
		}
	}
}
