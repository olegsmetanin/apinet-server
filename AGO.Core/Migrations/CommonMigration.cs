using System;
using AGO.Core.Migration;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using FluentMigrator;

namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 03), Tags(MigrationTags.ProjectDb)]
	public class CommonMigration: FluentMigrator.Migration
	{
		public override void Up()
		{
			var provider = ApplicationContext as string;
			// ReSharper disable once InconsistentNaming
			var use_citext = provider != null && provider.StartsWith("postgres", StringComparison.InvariantCultureIgnoreCase);

			Create.CoreModelTable<ProjectMemberModel>()
				.WithValueColumn<ProjectMemberModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ProjectMemberModel>(m => m.UserId)
				.WithValueColumn<ProjectMemberModel>(m => m.FullName, use_citext)
				.WithValueColumn<ProjectMemberModel>(m => m.RolesString)
				.WithValueColumn<ProjectMemberModel>(m => m.CurrentRole)
				.WithValueColumn<ProjectMemberModel>(m => m.UserPriority);

			Create.CoreModelTable<UserFilterModel>()
				.WithValueColumn<UserFilterModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.Name, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.GroupName, use_citext)
				.WithValueColumn<UserFilterModel>(m => m.Filter)
				.WithValueColumn<UserFilterModel>(m => m.OwnerId);

			//Custom props
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

			//Reports
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

			//Activity
			Create.SecureModelTable<ActivityRecordModel>()
				.WithValueColumn<ActivityRecordModel>(m => m.ProjectCode, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemType, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.AdditionalInfo, use_citext)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemId)
				.WithValueColumn<ActivityRecordModel>(m => m.ItemName, use_citext);

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
			Delete.ModelTable<ActivityRecordModel>();
			Delete.ModelTable<CustomPropertyInstanceModel>();
			Delete.ModelTable<CustomPropertyTypeModel>();
			Delete.ModelTable<ReportTaskModel>();
			Delete.ModelTable<ReportSettingModel>();
			Delete.ModelTable<ReportTemplateModel>();
			Delete.ModelTable<ProjectMemberModel>();
			Delete.ModelTable<UserFilterModel>();
		}
	}
}
