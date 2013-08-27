using AGO.Core.Model;
using AGO.Core.Model.Security;
using AGO.Hibernate.Migration;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;

namespace AGO.Core.Migrations
{
	public static class Extensions
	{
		public static ICreateTableWithColumnSyntax DocstoreModelTable<TModel>(
			this ICreateExpressionRoot root)
			where TModel : IDocstoreModel
		{
			return root.DocstoreModelTable<TModel>(Hibernate.Migration.Extensions.TableName<TModel>());
		}

		public static ICreateTableWithColumnSyntax DocstoreModelTable<TModel>(
			this ICreateExpressionRoot root,
			string tableName)
			where TModel : IDocstoreModel
		{
			return root.ModelTable<TModel>(tableName)
				.WithValueColumn<TModel>(m => m.CreationTime);
		}

		public static ICreateTableWithColumnSyntax SecureModelTable<TModel>(
			this ICreateExpressionRoot root)
			where TModel : ISecureModel
		{
			return root.SecureModelTable<TModel>(Hibernate.Migration.Extensions.TableName<TModel>());
		}

		public static ICreateTableWithColumnSyntax SecureModelTable<TModel>(
			this ICreateExpressionRoot root,
			string tableName)
			where TModel : ISecureModel
		{
			return root.DocstoreModelTable<TModel>(tableName)
				.WithRefColumn<TModel>(m => m.Creator)
				.WithValueColumn<TModel>(m => m.LastChangeTime)
				.WithRefColumn<TModel>(m => m.LastChanger);			
		}
	}
}
