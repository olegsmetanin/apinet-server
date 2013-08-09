using AGO.Docstore.Model.Dictionary;
using AGO.Docstore.Model.Documents;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Migration;
using FluentMigrator;

namespace AGO.Docstore.Migrations
{
	[Migration(10000)]
	public class InitialMigration : Migration
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

			Create.SecureModelTable<DocumentAddresseeModel>()
				.WithValueColumn<DocumentAddresseeModel>(m => m.ProjectCode)
				.WithValueColumn<DocumentAddresseeModel>(m => m.Name)
				.WithRefColumn<DocumentAddresseeModel>(m => m.Parent);

			Create.SecureModelTable<DocumentCategoryModel>()
				.WithValueColumn<DocumentCategoryModel>(m => m.ProjectCode)
				.WithValueColumn<DocumentCategoryModel>(m => m.Name)
				.WithRefColumn<DocumentCategoryModel>(m => m.Parent);

			Create.SecureModelTable<DocumentStatusModel>()
				.WithValueColumn<DocumentStatusModel>(m => m.ProjectCode)
				.WithValueColumn<DocumentStatusModel>(m => m.Name)
				.WithValueColumn<DocumentStatusModel>(m => m.Description);

			Create.SecureModelTable<DocumentModel>()
				.WithValueColumn<DocumentModel>(m => m.SeqNumber)
				.WithValueColumn<DocumentModel>(m => m.DocumentType)
				.WithValueColumn<DocumentModel>(m => m.Annotation)
				.WithValueColumn<DocumentModel>(m => m.Content)
				.WithValueColumn<DocumentModel>(m => m.Date)
				.WithValueColumn<DocumentModel>(m => m.Number)
				.WithValueColumn<DocumentModel>(m => m.SourceDocUrl)
				.WithValueColumn<DocumentModel>(m => m.SourceDocDate)
				.WithValueColumn<DocumentModel>(m => m.SourceDocNumber)
				.WithRefColumn<DocumentModel>(m => m.Status);

			Create.Table("UserModelToDepartmentModel")
				.WithColumn("UserId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_UserId", "UserModel", "Id")
				.WithColumn("DepartmentId").AsGuid().NotNullable()
					.ForeignKey("FK_UserModelToDepartmentModel_DepartmentId", "DepartmentModel", "Id");

			Create.Table("DocumentModelToDocumentAddresseeModel")
				.WithColumn("DocumentId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentAddresseeModel_DocumentId", "DocumentModel", "Id")
				.WithColumn("DocumentAddresseeId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentAddresseeModel_DocumentAddresseeId", "DocumentAddresseeModel", "Id");

			Create.Table("DocumentModelToDocumentCategoryModel")
				.WithColumn("DocumentId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentCategoryModel_DocumentId", "DocumentModel", "Id")
				.WithColumn("DocumentCategoryId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentCategoryModel_DocumentCategoryId", "DocumentCategoryModel", "Id");
		}

		public override void Down()
		{
		}
	}
}
