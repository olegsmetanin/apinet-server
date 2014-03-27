using System;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Configuration;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Migration;


namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 01)]
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

			Create.CoreModelTable<UserFilterModel>()
				.WithValueColumn<UserFilterModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.Name, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.GroupName, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.Filter)
				.WithValueColumn<UserFilterModel>(m => m.OwnerId);

			Create.CoreModelTable<TagModel>()
				.WithValueColumn<TagModel>(m => m.OwnerId)
				.WithValueColumn<TagModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<TagModel>(m => m.Name, use_citext)
				.WithValueColumn<TagModel>(m => m.FullName, use_citext)
				.WithRefColumn<TagModel>(m => m.Parent);

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

			Create.CoreModelTable<ProjectMemberModel>()
				.WithValueColumn<ProjectMemberModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ProjectMemberModel>(m => m.UserId)
				.WithValueColumn<ProjectMemberModel>(m => m.FullName, use_citext)
				.WithValueColumn<ProjectMemberModel>(m => m.RolesString)
				.WithValueColumn<ProjectMemberModel>(m => m.CurrentRole)
				.WithValueColumn<ProjectMemberModel>(m => m.UserPriority);

			Create.CoreModelTable<ProjectToTagModel>()
				.WithRefColumn<ProjectToTagModel>(m => m.Project)
				.WithRefColumn<ProjectToTagModel>(m => m.Tag);

			Create.SecureModelTable<CustomPropertyTypeModel>()
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Name, use_citext)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.FullName, use_citext)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Format)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ValueType)
				.WithRefColumn<CustomPropertyTypeModel, CustomPropertyTypeModel>(m => m.Parent);

			Create.SecureModelTable<CustomPropertyInstanceModel>()
				.WithRefColumn<CustomPropertyInstanceModel, CustomPropertyTypeModel>(m => m.PropertyType)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.StringValue, use_citext)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.NumberValue)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.DateValue);

			Create.CoreModelTable<ReportTemplateModel>()
				.WithValueColumn<ReportTemplateModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ReportTemplateModel>(m => m.Name, use_citext)
				.WithBinaryColumn<ReportTemplateModel>(m => m.Content)
				.WithValueColumn<ReportTemplateModel>(m => m.LastChange);
			Create.CoreModelTable<ReportSettingModel>()
				.WithValueColumn<ReportTemplateModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ReportSettingModel>(m => m.Name, use_citext)
				.WithValueColumn<ReportSettingModel>(m => m.TypeCode, use_citext)
				.WithValueColumn<ReportSettingModel>(m => m.GeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.DataGeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.ReportParameterType)
				.WithRefColumn<ReportSettingModel>(m => m.ReportTemplate);
			Create.SecureModelTable<ReportTaskModel>()
				.WithValueColumn<ReportTaskModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ReportTaskModel>(m => m.Name, use_citext)
				.WithRefColumn<ReportTaskModel>(m => m.ReportSetting)
				.WithValueColumn<ReportTaskModel>(m => m.Parameters)
				.WithValueColumn<ReportTaskModel>(m => m.Culture)
				.WithValueColumn<ReportTaskModel>(m => m.State)
				.WithValueColumn<ReportTaskModel>(m => m.DataGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.ReportGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.StartedAt)
				.WithValueColumn<ReportTaskModel>(m => m.CompletedAt)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorMsg, use_citext)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorDetails, use_citext)
				.WithBinaryColumn<ReportTaskModel>(m => m.ResultContent)
				.WithValueColumn<ReportTaskModel>(m => m.ResultName, use_citext)
				.WithValueColumn<ReportTaskModel>(m => m.ResultContentType, use_citext)
				.WithValueColumn<ReportTaskModel>(m => m.ResultUnread);
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

			Create.SecureModelTable<ActivityRecordModel>()
				.WithValueColumn<ActivityRecordModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemType, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemName, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemId);

			Alter.ModelTable<ActivityRecordModel>()
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.Attribute, use_citext)
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.OldValue, use_citext)
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.NewValue, use_citext);

			Alter.ModelTable<ActivityRecordModel>()
				.AddValueColumn<RelatedChangeActivityRecordModel>(m => m.RelatedItemType, use_citext)
				.AddValueColumn<RelatedChangeActivityRecordModel>(m => m.RelatedItemName, use_citext)
				.AddValueColumn<RelatedChangeActivityRecordModel>(m => m.RelatedItemId)
				.AddValueColumn<RelatedChangeActivityRecordModel>(m => m.ChangeType);
		}

		public override void Down()
		{
		}
	}
}
