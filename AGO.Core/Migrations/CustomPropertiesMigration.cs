using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Documents;
using AGO.Core.Model.Dictionary.OrgStructure;
using AGO.Core.Model.Documents;
using AGO.Hibernate.Migration;
using FluentMigrator;

namespace AGO.Core.Migrations
{
	[Migration(10100)]
	public class CustomPropertiesMigration : Migration
	{
		public override void Up()
		{
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

			Alter.ModelTable<DocumentCustomPropertyModel>()
			     .AddRefColumn<DocumentCustomPropertyModel>(m => m.Document);

			Alter.ModelTable<DocumentAddresseeModel>()
			     .AddValueColumn<DocumentAddresseeModel>(m => m.FullName);

			Alter.ModelTable<DocumentCategoryModel>()
				 .AddValueColumn<DocumentCategoryModel>(m => m.FullName);

			Alter.ModelTable<DepartmentModel>()
				.AlterColumn("FullName").AsString(1024).Nullable();

		}

		public override void Down()
		{
			Alter.ModelTable<DepartmentModel>()
				.AlterColumn("FullName").AsString(128).Nullable();

			Delete
				.Column<DocumentCategoryModel>(m => m.FullName)
				.FromModelTable<DocumentCategoryModel>();

			Delete
				.Column<DocumentAddresseeModel>(m => m.FullName)
				.FromModelTable<DocumentAddresseeModel>();

			Delete.ModelTable<CustomPropertyInstanceModel>();

			Delete.ModelTable<CustomPropertyTypeModel>();
		}
	}
}
