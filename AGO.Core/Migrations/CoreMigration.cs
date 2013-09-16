using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Core.Migration;

namespace AGO.Core.Migrations
{
	[MigrationVersion(2013, 09, 01, 01)]
	public class CoreMigration : FluentMigrator.Migration
	{
		public override void Up()
		{
			Create.DocstoreModelTable<UserModel>();

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

			Create.Table("UserModelToDepartmentModel")
				.WithColumn("UserId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_UserId", "UserModel", "Id")
				.WithColumn("DepartmentId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_DepartmentId", "DepartmentModel", "Id");

			Create.SecureModelTable<CustomPropertyTypeModel>()
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ProjectCode)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Name)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.FullName)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.Format)
				.WithValueColumn<CustomPropertyTypeModel>(m => m.ValueType)
				.WithRefColumn<CustomPropertyTypeModel>(m => m.Parent);

			Create.SecureModelTable<CustomPropertyInstanceModel>()
				.WithRefColumn<CustomPropertyInstanceModel>(m => m.PropertyType)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.StringValue)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.NumberValue)
				.WithValueColumn<CustomPropertyInstanceModel>(m => m.DateValue);

			Create.SecureModelTable<TagModel>()
				.WithValueColumn<TagModel>(m => m.ProjectCode)
				.WithValueColumn<TagModel>(m => m.Name)
				.WithValueColumn<TagModel>(m => m.FullName)
				.WithRefColumn<TagModel>(m => m.Parent)
				.WithRefColumn<TagModel>(m => m.Owner);
		}

		public override void Down()
		{
		}
	}
}
