using AGO.DocManagement.Model.Dictionary.Documents;
using AGO.DocManagement.Model.Documents;
using AGO.Core.Migration;
using FluentMigrator;

namespace AGO.DocManagement.Migrations
{
	[MigrationVersion(2013, 09, 10, 01)]
	public class DocumentsMigration : Migration
	{
		internal const string MODULE_SCHEMA = "DocManagement";

		public override void Up()
		{
			Create.SecureModelTable<DocumentAddresseeModel>()
				.WithValueColumn<DocumentAddresseeModel>(m => m.ProjectCode)
				.WithValueColumn<DocumentAddresseeModel>(m => m.Name)
				.WithValueColumn<DocumentAddresseeModel>(m => m.FullName)
				.WithRefColumn<DocumentAddresseeModel>(m => m.Parent);

			Create.SecureModelTable<DocumentCategoryModel>()
				.WithValueColumn<DocumentCategoryModel>(m => m.ProjectCode)
				.WithValueColumn<DocumentCategoryModel>(m => m.Name)
				.WithValueColumn<DocumentCategoryModel>(m => m.FullName)
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

			Create.Table("DocumentModelToDocumentAddresseeModel").InSchema(MODULE_SCHEMA)
				.WithColumn("DocumentId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentAddresseeModel_DocumentId", MODULE_SCHEMA, "DocumentModel", "Id")
				.WithColumn("DocumentAddresseeId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentAddresseeModel_DocumentAddresseeId", MODULE_SCHEMA, "DocumentAddresseeModel", "Id");

			Create.Table("DocumentModelToDocumentCategoryModel").InSchema(MODULE_SCHEMA)
				.WithColumn("DocumentId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentCategoryModel_DocumentId", MODULE_SCHEMA, "DocumentModel", "Id")
				.WithColumn("DocumentCategoryId").AsGuid().NotNullable()
					.ForeignKey("FK_DocumentModelToDocumentCategoryModel_DocumentCategoryId", MODULE_SCHEMA, "DocumentCategoryModel", "Id");
			
			Create.SecureModelTable<DocumentStatusHistoryModel>()
				.WithValueColumn<DocumentStatusHistoryModel>(m => m.StartDate)
				.WithValueColumn<DocumentStatusHistoryModel>(m => m.EndDate)
				.WithRefColumn<DocumentStatusHistoryModel>(m => m.Document)
				.WithRefColumn<DocumentStatusHistoryModel>(m => m.Status);

			Alter.ModelTable<DocumentCustomPropertyModel>()
				 .AddRefColumn<DocumentCustomPropertyModel>(m => m.Document);

		}

		public override void Down()
		{

		}
	}
}
