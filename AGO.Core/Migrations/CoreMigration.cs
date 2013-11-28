using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Migration;

namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 01)]
	public class CoreMigration : FluentMigrator.Migration
	{
		internal const string MODULE_SCHEMA = "Core";

		public override void Up()
		{
			Create.CoreModelTable<UserModel>();

			Alter.ModelTable<UserModel>()
				.AddRefColumn<UserModel>(m => m.Creator)
				.AddValueColumn<UserModel>(m => m.LastChangeTime)
				.AddRefColumn<UserModel>(m => m.LastChanger)

				.AddValueColumn<UserModel>(m => m.Login)
				.AddValueColumn<UserModel>(m => m.PwdHash)
				.AddValueColumn<UserModel>(m => m.Active)
				.AddValueColumn<UserModel>(m => m.Name)
				.AddValueColumn<UserModel>(m => m.LastName)
				.AddValueColumn<UserModel>(m => m.MiddleName)
				.AddValueColumn<UserModel>(m => m.FullName)
				.AddValueColumn<UserModel>(m => m.FIO)
				.AddValueColumn<UserModel>(m => m.WhomFIO)
				.AddValueColumn<UserModel>(m => m.JobName)
				.AddValueColumn<UserModel>(m => m.WhomJobName)
				.AddValueColumn<UserModel>(m => m.SystemRole);

			Create.SecureModelTable<DepartmentModel>()
				.WithValueColumn<DepartmentModel>(m => m.ProjectCode)
				.WithValueColumn<DepartmentModel>(m => m.Name)
				.WithValueColumn<DepartmentModel>(m => m.FullName)
				.WithRefColumn<DepartmentModel>(m => m.Parent);

			Create.Table("UserModelToDepartmentModel").InSchema(MODULE_SCHEMA)
				.WithColumn("UserId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_UserId", MODULE_SCHEMA, "UserModel", "Id")
				.WithColumn("DepartmentId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_DepartmentId", MODULE_SCHEMA, "DepartmentModel", "Id");

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
				.WithRefColumn<TagModel>(m => m.Parent)
				.WithRefColumn<TagModel>(m => m.Owner);

			Create.CoreModelTable<ReportingServiceDescriptorModel>()
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.Name)
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.EndPoint)
				.WithValueColumn<ReportingServiceDescriptorModel>(m => m.LongRunning);
			Create.CoreModelTable<ReportTemplateModel>()
				.WithValueColumn<ReportTemplateModel>(m => m.Name)
				.WithBinaryColumn<ReportTemplateModel>(m => m.TemplateContent)
				.WithValueColumn<ReportTemplateModel>(m => m.LastChange);
			Create.CoreModelTable<ReportSettingModel>()
				.WithValueColumn<ReportSettingModel>(m => m.Name)
				.WithValueColumn<ReportSettingModel>(m => m.GeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.DataGeneratorType)
				.WithValueColumn<ReportSettingModel>(m => m.ReportParameterType)
				.WithRefColumn<ReportSettingModel>(m => m.ReportTemplate);
			Create.CoreModelTable<ReportTaskModel>()
				.WithValueColumn<ReportTaskModel>(m => m.Name)
				.WithRefColumn<ReportTaskModel>(m => m.ReportSetting)
				.WithRefColumn<ReportTaskModel>(m => m.ReportingService)
				.WithValueColumn<ReportTaskModel>(m => m.Parameters)
				.WithValueColumn<ReportTaskModel>(m => m.State)
				.WithValueColumn<ReportTaskModel>(m => m.DateGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.ReportGenerationProgress)
				.WithValueColumn<ReportTaskModel>(m => m.StartedAt)
				.WithValueColumn<ReportTaskModel>(m => m.CompletedAt)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorMsg)
				.WithValueColumn<ReportTaskModel>(m => m.ErrorDetails)
				.WithBinaryColumn<ReportTaskModel>(m => m.ResultContent)
				.WithValueColumn<ReportTaskModel>(m => m.ResultContentType);
		}

		public override void Down()
		{
		}
	}
}
