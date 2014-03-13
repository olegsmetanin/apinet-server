﻿using AGO.Core.Model.Activity;
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
			Create.CoreModelTable<UserModel>();

			Alter.ModelTable<UserModel>()
				.AddRefColumn<UserModel>(m => m.Creator)
				.AddValueColumn<UserModel>(m => m.LastChangeTime)
				.AddRefColumn<UserModel>(m => m.LastChanger)

				.AddValueColumn<UserModel>(m => m.Email)
				.AddValueColumn<UserModel>(m => m.Active)
				.AddValueColumn<UserModel>(m => m.FirstName)
				.AddValueColumn<UserModel>(m => m.LastName)
				.AddValueColumn<UserModel>(m => m.FullName)
				.AddValueColumn<UserModel>(m => m.SystemRole)
				.AddValueColumn<UserModel>(m => m.AvatarUrl)
				.AddValueColumn<UserModel>(m => m.OAuthProvider)
				.AddValueColumn<UserModel>(m => m.OAuthUserId);

			Create.CoreModelTable<OAuthDataModel>()
				.WithValueColumn<OAuthDataModel>(m => m.RedirectUrl)
				.WithValueColumn<TwitterOAuthDataModel>(m => m.Token)
				.WithValueColumn<TwitterOAuthDataModel>(m => m.TokenSecret);

			Create.CoreModelTable<UserFilterModel>()
				.WithValueColumn<UserFilterModel>(m => m.Name)
				.WithValueColumn<UserFilterModel>(m => m.GroupName)
				.WithValueColumn<UserFilterModel>(m => m.Filter)
				.WithRefColumn<UserFilterModel>(m => m.User);

			Create.SecureModelTable<CustomPropertyTypeModel>()
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ProjectCode)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Name)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.FullName)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Format)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ValueType)
				.WithRefColumn<CustomPropertyTypeModel, CustomPropertyTypeModel>(m => m.Parent);

			Create.SecureModelTable<CustomPropertyInstanceModel>()
				.WithRefColumn<CustomPropertyInstanceModel,CustomPropertyTypeModel>(m => m.PropertyType)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.StringValue)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.NumberValue)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.DateValue);

			Create.SecureModelTable<TagModel>()
				.WithValueColumn<TagModel>(m => m.ProjectCode)
				.WithValueColumn<TagModel>(m => m.Name)
				.WithValueColumn<TagModel>(m => m.FullName)
				.WithRefColumn<TagModel>(m => m.Parent);

			Create.SecureModelTable<ProjectTypeModel>()
				.WithValueColumn<ProjectTypeModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectTypeModel>(m => m.Name)
				.WithValueColumn<ProjectTypeModel>(m => m.Description)
				.WithValueColumn<ProjectTypeModel>(m => m.Module);

			Create.SecureModelTable<ProjectModel>()
				.WithValueColumn<ProjectModel>(m => m.ProjectCode).Unique("IX_ProjectModel_ProjectCode")
				.WithValueColumn<ProjectModel>(m => m.Name)
				.WithValueColumn<ProjectModel>(m => m.Description)
				.WithRefColumn<ProjectModel>(m => m.Type)
				.WithValueColumn<ProjectModel>(m => m.VisibleForAll)
				.WithValueColumn<ProjectModel>(m => m.Status);

			Create.CoreModelTable<ProjectMembershipModel>()
				.WithRefColumn<ProjectMembershipModel>(pm => pm.Project)
				.WithRefColumn<ProjectMembershipModel>(m => m.User);

			Create.SecureModelTable<ProjectStatusHistoryModel>()
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Start)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Finish)
				.WithRefColumn<ProjectStatusHistoryModel>(m => m.Project)
				.WithValueColumn<ProjectStatusHistoryModel>(m => m.Status);

			Create.CoreModelTable<ProjectMemberModel>()
				.WithValueColumn<ProjectMemberModel>(m => m.ProjectCode)
				.WithValueColumn<ProjectMemberModel>(m => m.UserId)
				.WithValueColumn<ProjectMemberModel>(m => m.FullName)
				.WithValueColumn<ProjectMemberModel>(m => m.RolesString)
				.WithValueColumn<ProjectMemberModel>(m => m.CurrentRole)
				.WithValueColumn<ProjectMemberModel>(m => m.UserPriority);

			Create.SecureModelTable<ProjectToTagModel>()
				.WithRefColumn<ProjectToTagModel>(m => m.Project)
				.WithRefColumn<ProjectToTagModel>(m => m.Tag);

			Create.CoreModelTable<ReportingServiceDescriptorModel>()
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.Name)
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.EndPoint)
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.LongRunning);
			Create.CoreModelTable<ReportTemplateModel>()
				.WithValueColumn<ReportTemplateModel>(m => m.Name)
				.WithBinaryColumn<ReportTemplateModel>(m => m.Content)
				.WithValueColumn<ReportTemplateModel>(m => m.LastChange);
			Create.CoreModelTable<ReportSettingModel>()
				.WithValueColumn<ReportSettingModel>(m => m.Name)
				.WithValueColumn<ReportSettingModel>(m => m.TypeCode)
				.WithValueColumn<ReportSettingModel>(m => m.GeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.DataGeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.ReportParameterType)
				.WithRefColumn<ReportSettingModel>(m => m.ReportTemplate);
			Create.SecureModelTable<ReportTaskModel>()
				.WithValueColumn<ReportTaskModel>(m => m.Project)
				.WithValueColumn<ReportTaskModel>(m => m.Name)
				.WithRefColumn<ReportTaskModel>(m => m.ReportSetting)
				.WithValueColumn<ReportTaskModel>(m => m.Parameters)
				.WithValueColumn<ReportTaskModel>(m => m.Culture)
				.WithValueColumn<ReportTaskModel>(m => m.State)
				.WithValueColumn<ReportTaskModel>(m => m.DataGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.ReportGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.StartedAt)
				.WithValueColumn<ReportTaskModel>(m => m.CompletedAt)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorMsg)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorDetails)
				.WithBinaryColumn<ReportTaskModel>(m => m.ResultContent)
				.WithValueColumn<ReportTaskModel>(m => m.ResultName)
				.WithValueColumn<ReportTaskModel>(m => m.ResultContentType)
				.WithValueColumn<ReportTaskModel>(m => m.ResultUnread);
			//Hash token:login, that will be used in socket.io for sending messages
			//to particulat user
			Create.Table("TokenToLogin").InSchema(MODULE_SCHEMA)
				.WithColumn("Token").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("Login").AsString(UserModel.EMAIL_SIZE).NotNullable()
				.WithColumn("CreatedAt").AsDateTime().NotNullable();

			//WorkQueue, will be used later as source for all async worker (now only for reporting service)
			Create.Table("WorkQueue").InSchema(MODULE_SCHEMA)
				.WithColumn("TaskType").AsString(128).NotNullable()
				.WithColumn("TaskId").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("Project").AsString(ProjectModel.PROJECT_CODE_SIZE).NotNullable()
				.WithColumn("User").AsString(UserModel.EMAIL_SIZE).NotNullable()
				.WithColumn("CreateDate").AsDateTime().NotNullable()
				.WithColumn("PriorityType").AsInt32().NotNullable()
				.WithColumn("UserPriority").AsInt32().NotNullable();

			Create.SecureModelTable<ActivityRecordModel>()
				.WithValueColumn<ActivityRecordModel>(m => m.ProjectCode)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemType)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemName)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemId);

			Alter.ModelTable<ActivityRecordModel>()
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.Attribute)
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.OldValue)
				.AddValueColumn<AttributeChangeActivityRecordModel>(m => m.NewValue);

			Alter.ModelTable<ActivityRecordModel>()
				.AddValueColumn<CollectionChangeActivityRecordModel>(m => m.RelatedItemType)
				.AddValueColumn<CollectionChangeActivityRecordModel>(m => m.RelatedItemName)
				.AddValueColumn<CollectionChangeActivityRecordModel>(m => m.RelatedItemId)
				.AddValueColumn<CollectionChangeActivityRecordModel>(m => m.ChangeType);
		}

		public override void Down()
		{
		}
	}
}
