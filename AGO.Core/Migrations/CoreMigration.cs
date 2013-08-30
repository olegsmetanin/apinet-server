using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.OrgStructure;
using AGO.Core.Model.Security;
using AGO.Core.Migration;
using FluentMigrator;

namespace AGO.Core.Migrations
{
	[Migration(9000)]
	public class CoreMigration : FluentMigrator.Migration
	{
		public override void Up()
		{
			Create.DocstoreModelTable<UserModel>();

			Create.SecureModelTable<UserGroupModel>()
				.WithValueColumn<UserGroupModel>(m => m.Name)
				.WithValueColumn<UserGroupModel>(m => m.Description);

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
				.AddRefColumn<UserModel>(m => m.Group);

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

			Create.DocstoreModelTable<RoleModel>()
				.WithValueColumn<RoleModel>(m => m.Name)
				.WithValueColumn<RoleModel>(m => m.Description);

			Create.Table("UserModelToRoleModel")
				.WithColumn("UserId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToRoleModel_UserId", "UserModel", "Id")
				.WithColumn("RoleId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToRoleModel_RoleId", "RoleModel", "Id");

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
		}

		public override void Down()
		{
		}
	}
}
