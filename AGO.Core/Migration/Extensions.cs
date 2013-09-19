using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AGO.Core.Model;
using AGO.Core.Model.Security;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Column;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Lob;

namespace AGO.Core.Migration
{
	public static class Extensions
	{
		#region Constants

		private const int _DefaultStringIdColumnSize = 64;

		private const int _DefaultEnumColumnSize = 64;

		private const int _DefaultTypeColumnSize = 256;

		#endregion

		#region Creation

		public static ICreateTableWithColumnSyntax ModelTable<TModel>(this ICreateExpressionRoot root)
		{
			var type = typeof(TModel);

			var table = root.Table(TableName<TModel>()).InSchema(SchemaName<TModel>());

			var idProperty = type.GetProperty("Id");
			if (idProperty != null)
			{
				var length = _DefaultStringIdColumnSize;
				var notLonger = idProperty.FirstAttribute<NotLongerAttribute>(true);
				if (notLonger != null)
					length = notLonger.Limit > 0 ? notLonger.Limit : length;

				var idType = idProperty.PropertyType;
				if (typeof(string).IsAssignableFrom(idType))
					table = table.WithColumn("Id").AsString(length).NotNullable().PrimaryKey();
				else if (typeof(int?).IsAssignableFrom(idType))
					table = table.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();
				else if (typeof(Guid?).IsAssignableFrom(idType))
					table = table.WithColumn("Id").AsGuid().NotNullable().PrimaryKey();
			}

			var versionProperty = type.GetProperty("ModelVersion") ?? type.GetProperty("TimeStamp");
			var versionAttribute = versionProperty != null ? versionProperty.FirstAttribute<ModelVersionAttribute>(true) : null;
			var notMappedAttribute = versionProperty != null ? versionProperty.FirstAttribute<NotMappedAttribute>(true) : null;
			if (versionProperty != null && versionAttribute!=null && notMappedAttribute == null)
			{
				var versionType = versionProperty.PropertyType;
				if (typeof(DateTime?).IsAssignableFrom(versionType))
					table = table.WithColumn(versionProperty.Name).AsDateTime().Nullable();
				else if (typeof(int?).IsAssignableFrom(versionType))
					table = table.WithColumn(versionProperty.Name).AsInt32().Nullable();
				else if (typeof(Guid?).IsAssignableFrom(versionType))
					table = table.WithColumn(versionProperty.Name).AsGuid().Nullable();
			}
		
			var tablePerSubclassAttribute = type.FirstAttribute<TablePerSubclassAttribute>(true);
			if (tablePerSubclassAttribute != null)
				table = table.WithColumn(tablePerSubclassAttribute.DiscriminatorColumn).AsString(128);

			return table;
		}

		public static ICreateTableColumnAsTypeSyntax WithColumn<TModel>(
            this ICreateTableWithColumnSyntax self, Expression<Func<TModel, object>> expression)
		{
			return self.WithColumn(ColumnName(expression));
		}

	    public static ICreateTableColumnOptionOrWithColumnSyntax WithValueColumn<TModel>(
            this ICreateTableWithColumnSyntax self, Expression<Func<TModel, object>> expression)
		{
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();
			
			var propertyType = propertyInfo.PropertyType;
			if (propertyType.IsNullable())
				propertyType = propertyType.GetGenericArguments()[0];
			if(!propertyType.IsValueType && typeof(string) != propertyType)
				throw new Exception("propertyType is not value or string");

			var start = self.WithColumn(propertyInfo.Name);
			ICreateTableColumnOptionOrWithColumnSyntax next = null;

			if (typeof (Guid) == propertyType)
				next = start.AsGuid();
			else if (typeof (DateTime) == propertyType)
				next = start.AsDateTime();
			else if (typeof (bool) == propertyType)
				next = start.AsBoolean();
			else if (propertyType.IsEnum)
				next = start.AsString(_DefaultEnumColumnSize);
			else if (typeof(byte) == propertyType || typeof(sbyte) == propertyType)
				next = start.AsByte();
			else if (typeof(char) == propertyType)
				next = start.AsFixedLengthString(1);
			else if (typeof (Decimal) == propertyType)
				next = start.AsDecimal();
			else if (typeof(float) == propertyType)
				next = start.AsFloat();
			else if (typeof (double) == propertyType)
				next = start.AsDouble();		
			else if (typeof(short) == propertyType || typeof(ushort) == propertyType)
				next = start.AsInt16();
			else if (typeof(int) == propertyType || typeof(uint) == propertyType)
				next = start.AsInt32();
			else if (typeof(long) == propertyType || typeof(ulong) == propertyType)
				next = start.AsInt64();
			else if (typeof (string) == propertyType)
			{
				var length = Int32.MaxValue;
				var notLonger = propertyInfo.FirstAttribute<NotLongerAttribute>(true);
				if (notLonger != null)
					length = notLonger.Limit > 0 ? notLonger.Limit : length;
				next = start.AsString(length);
			}

			if(next == null)
				throw new Exception("Unexpected propertyType");
			return next.ColumnOptions<TModel>(propertyInfo);
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithBinaryColumn<TModel>(
            this ICreateTableWithColumnSyntax self, Expression<Func<TModel, Blob>> expression)
		{
			var length = Int32.MaxValue;
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();
			var notLonger = propertyInfo.FirstAttribute<NotLongerAttribute>(true);
			if (notLonger != null)
				length = notLonger.Limit > 0 ? notLonger.Limit : length;
			return self.WithColumn(propertyInfo.Name).AsBinary(length).ColumnOptions<TModel>(propertyInfo);
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithTypeColumn<TModel>(
            this ICreateTableWithColumnSyntax self, Expression<Func<TModel, Type>> expression)
		{
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();
			return self.WithColumn(propertyInfo.Name).AsString(_DefaultTypeColumnSize).ColumnOptions<TModel>(propertyInfo);
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithRefColumn<TModel, TForeignModel>(
            this ICreateTableWithColumnSyntax self, Expression<Func<TModel, object>> expression)
		{
			return self.WithRefColumn(expression, true, typeof(TForeignModel));
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithRefColumn<TModel>(
			this ICreateTableWithColumnSyntax self, 
			Expression<Func<TModel, object>> expression, 
			bool isForeignKey = true, 
			Type foreignType = null)
		{
			var columnName = ColumnName(expression);
			var propertyInfo = expression.PropertyInfoFromExpression();
			foreignType = foreignType ?? propertyInfo.PropertyType;

			var idProperty = foreignType.GetProperty("Id") ?? propertyInfo;

			var idType = idProperty.PropertyType;
			ICreateTableColumnOptionOrWithColumnSyntax result = null;
			if (typeof(string).IsAssignableFrom(idType))
			{
				var length = _DefaultStringIdColumnSize;
				var notLonger = idProperty.FirstAttribute<NotLongerAttribute>(true);
				if (notLonger != null)
					length = notLonger.Limit > 0 ? notLonger.Limit : length;
				result = self.WithColumn(columnName).AsString(length);
			}
			if (typeof(int?).IsAssignableFrom(idType))
				result = self.WithColumn(columnName).AsInt32();
			if (typeof(Guid?).IsAssignableFrom(idType))
				result = self.WithColumn(columnName).AsGuid();
			if (result == null)
				throw new Exception(string.Format("Unexpected ref property type in model type \"{0}\"", foreignType));
			if (isForeignKey)
			{
			    var fkName = "FK_" + TableName<TModel>() + "_" + ColumnName(expression);
				result.ForeignKey(fkName, SchemaName(foreignType), TableName(foreignType), "Id");
			}
			return result.ColumnOptions<TModel>(propertyInfo);
		}

		public static ICreateExpressionRoot Indexes<TModel>(
			this ICreateExpressionRoot self, params Expression<Func<TModel, object>>[] expressions)
		{
			foreach (var expression in expressions)
			{
				self.Index("IX_" + TableName<TModel>() + "_" + ColumnName(expression))
					.OnTable(TableName<TModel>()).OnColumn(ColumnName(expression)).Ascending().WithOptions().NonClustered();
			}
			return self;
		}

		public static ICreateExpressionRoot MultiColumnIndex<TModel>(
			this ICreateExpressionRoot self, params Expression<Func<TModel, object>>[] expressions)
		{
			var indexName = "IX_" + TableName<TModel>();
			indexName += expressions.Aggregate("", (current, expression) => current + ("_" + ColumnName(expression)));

			var tempResult = self.Index(indexName).OnTable(TableName<TModel>());
			foreach (var expression in expressions)
				tempResult.OnColumn(ColumnName(expression)).Ascending();

			tempResult.WithOptions().NonClustered();
			return self;
		}

		public static ICreateExpressionRoot UniqueIndex<TModel>(this ICreateExpressionRoot self, params Expression<Func<TModel, object>>[] expressions)
		{
			var indexName = "IX_" + TableName<TModel>();
			indexName += expressions.Aggregate("", (current, expression) => current + ("_" + ColumnName(expression)));

			var tempResult = self.Index(indexName).OnTable(TableName<TModel>());
			foreach (var expression in expressions)
			{
				tempResult.OnColumn(ColumnName(expression)).Ascending();
			}

			tempResult.WithOptions().Unique().WithOptions().NonClustered();
			return self;
		}

		#endregion

		#region Addition

		public static IAlterTableAddColumnOrAlterColumnSyntax ModelTable<TModel>(this IAlterExpressionRoot root)
		{
			return root.Table(TableName<TModel>()).InSchema(SchemaName<TModel>());
		}

		public static IAlterTableColumnAsTypeSyntax AddColumn<TModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self,
			Expression<Func<TModel, object>> expression)
		{
			return self.AddColumn(ColumnName(expression));
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AddValueColumn<TModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self, 
			Expression<Func<TModel, object>> expression)
		{
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();

			var propertyType = propertyInfo.PropertyType;
			if (propertyType.IsNullable())
				propertyType = propertyType.GetGenericArguments()[0];
			if (!propertyType.IsValueType && typeof(string) != propertyType)
				throw new Exception("propertyType is not value");

			var start = self.AddColumn(propertyInfo.Name);
			IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax next = null;

			if (typeof(Guid) == propertyType)
				next = start.AsGuid();
			else if (typeof(DateTime) == propertyType)
				next = start.AsDateTime();
			else if (typeof(bool) == propertyType)
				next = start.AsBoolean();
			else if (propertyType.IsEnum)
				next = start.AsString(_DefaultEnumColumnSize);
			else if (typeof(byte) == propertyType || typeof(sbyte) == propertyType)
				next = start.AsByte();
			else if (typeof(char) == propertyType)
				next = start.AsFixedLengthString(1);
			else if (typeof(Decimal) == propertyType)
				next = start.AsDecimal();
			else if (typeof(float) == propertyType)
				next = start.AsFloat();
			else if (typeof(double) == propertyType)
				next = start.AsDouble();
			else if (typeof(short) == propertyType || typeof(ushort) == propertyType)
				next = start.AsInt16();
			else if (typeof(int) == propertyType || typeof(uint) == propertyType)
				next = start.AsInt32();
			else if (typeof(long) == propertyType || typeof(ulong) == propertyType)
				next = start.AsInt64();
			else if (typeof(string) == propertyType)
			{
				var length = Int32.MaxValue;
				var notLonger = propertyInfo.FirstAttribute<NotLongerAttribute>(true);
				if (notLonger != null)
					length = notLonger.Limit > 0 ? notLonger.Limit : length;
				next = start.AsString(length);
			}

			if (next == null)
				throw new Exception("Unexpected propertyType");
			return next.ColumnOptions<TModel>(propertyInfo);
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AddTypeColumn<TModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self, Expression<Func<TModel, Type>> expression)
		{
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();
			return self.AddColumn(propertyInfo.Name).AsString(_DefaultTypeColumnSize).ColumnOptions<TModel>(propertyInfo);
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AddBinaryColumn<TModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self, Expression<Func<TModel, Blob>> expression)
		{
			var length = Int32.MaxValue;
			var propertyInfo = expression.PropertyInfoFromExpression();
			if (propertyInfo == null)
				throw new InvalidOperationException();
			var notLonger = propertyInfo.FirstAttribute<NotLongerAttribute>(true);
			if (notLonger != null)
				length = notLonger.Limit > 0 ? notLonger.Limit : length;
			return self.AddColumn(propertyInfo.Name).AsBinary(length).ColumnOptions<TModel>(propertyInfo);
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AddRefColumn<TModel, TForeignModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self, Expression<Func<TModel, object>> expression)
		{
			return self.AddRefColumn(expression, true, typeof(TForeignModel));
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AddRefColumn<TModel>(
			this IAlterTableAddColumnOrAlterColumnSyntax self, 
			Expression<Func<TModel, object>> expression, 
			bool isForeignKey = true,
			Type foreignType = null)
		{
			var columnName = ColumnName(expression);
			var propertyInfo = expression.PropertyInfoFromExpression();
			foreignType = foreignType ?? propertyInfo.PropertyType;

			var idProperty = foreignType.GetProperty("Id") ?? propertyInfo;

			var idType = idProperty.PropertyType;
			IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax result = null;
			if (typeof(string).IsAssignableFrom(idType))
			{
				var length = _DefaultStringIdColumnSize;
				var notLonger = idProperty.FirstAttribute<NotLongerAttribute>(true);
				if (notLonger != null)
					length = notLonger.Limit > 0 ? notLonger.Limit : length;
				result = self.AddColumn(columnName).AsString(length);
			}
			if (typeof(int?).IsAssignableFrom(idType))
				result = self.AddColumn(columnName).AsInt32();
			if (typeof(Guid?).IsAssignableFrom(idType))
				result = self.AddColumn(columnName).AsGuid();
			if (result == null)
				throw new Exception(string.Format("Unexpected ref property type in model type \"{0}\"", foreignType));
			if (isForeignKey)
			{
				var fkName = "FK_" + TableName<TModel>() + "_" + ColumnName(expression);
				result.ForeignKey(fkName, SchemaName(foreignType), TableName(foreignType), "Id");
			}
			return result.ColumnOptions<TModel>(propertyInfo);
		}

		#endregion

		#region Deletion

		public static void ModelTable<TModel>(this IDeleteExpressionRoot root)
		{
			root.Table(TableName<TModel>()).InSchema(SchemaName<TModel>());
		}

		public static IDeleteColumnFromTableSyntax Column<TModel>(
			this IDeleteExpressionRoot root, Expression<Func<TModel, object>> expression)
		{
			return root.Column(ColumnName(expression));
		}

		public static IDeleteColumnFromTableSyntax Column<TModel>(
			this IDeleteColumnFromTableSyntax self, Expression<Func<TModel, object>> expression)
		{
			return self.Column(ColumnName(expression));
		}

		public static void FromModelTable<TModel>(this IDeleteColumnFromTableSyntax self)
		{
			self.FromTable(TableName<TModel>()).InSchema(SchemaName<TModel>());
		}

		public static IDeleteDataSyntax FromModelTable<TModel>(this IDeleteExpressionRoot root)
		{
			return root.FromTable(TableName<TModel>()).InSchema(SchemaName<TModel>());
		}

		public static IDeleteExpressionRoot NullPropertyRows<TModel>(
			this IDeleteExpressionRoot root, Expression<Func<TModel, object>> expression)
		{
			root.FromTable(TableName<TModel>()).IsNull(ColumnName(expression));
			return root;
		}

		public static IDeleteExpressionRoot RefColumn<TModel>(
			this IDeleteExpressionRoot root, 
			Expression<Func<TModel, object>> expression)
		{
			root.ForeignKey("FK_" + TableName<TModel>() + "_" + ColumnName(expression))
				.OnTable(TableName<TModel>()).InSchema(SchemaName<TModel>());
			root.Column(expression).FromTable(TableName<TModel>()).InSchema(SchemaName<TModel>());
			return root;
		}

		public static IDeleteExpressionRoot Indexes<TModel>(
			this IDeleteExpressionRoot self, params Expression<Func<TModel, object>>[] expressions)
		{
			foreach (var expression in expressions)
			{
				self.Index("IX_" + TableName<TModel>() + "_" + ColumnName(expression))
					.OnTable(TableName<TModel>()).InSchema(SchemaName<TModel>()).OnColumn(ColumnName(expression));
			}
			return self;
		}

		public static IDeleteExpressionRoot MultiColumnIndex<TModel>(
			this IDeleteExpressionRoot self, params Expression<Func<TModel, object>>[] expressions)
		{
			var indexName = "IX_" + TableName<TModel>();
			indexName += expressions.Aggregate("", (current, expression) => current + ("_" + ColumnName(expression)));

			var tempResult = self.Index(indexName).OnTable(TableName<TModel>()).InSchema(SchemaName<TModel>());
			foreach (var expression in expressions)
				tempResult.OnColumn(ColumnName(expression));

			return self;
		}

		#endregion

		#region Docstore

        public static ICreateTableWithColumnSyntax DocstoreModelTable<TModel>(this ICreateExpressionRoot root)
			where TModel : IDocstoreModel
        {
			return root.ModelTable<TModel>().WithValueColumn<TModel>(m => m.CreationTime);
        }

        public static ICreateTableWithColumnSyntax SecureModelTable<TModel>(this ICreateExpressionRoot root)
			where TModel : ISecureModel
		{
			return root.DocstoreModelTable<TModel>()
				.WithRefColumn<TModel>(m => m.Creator)
				.WithValueColumn<TModel>(m => m.LastChangeTime)
                .WithRefColumn<TModel>(m => m.LastChanger);
		}

		#endregion

		#region Helper methods

		public static string SchemaName<TModel>()
		{
			return SchemaName(typeof(TModel));
		}

		public static string TableName<TModel>()
		{
			return TableName(typeof (TModel));
		}

		public static string SchemaName(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			var parts = type.Assembly.GetName().Name.Split('.');
			var schema = parts[parts.Length - 1];

			var tableAttribute = type.FirstAttribute<TableAttribute>(false);
			if (tableAttribute != null && !tableAttribute.SchemaName.IsNullOrWhiteSpace())
				schema = tableAttribute.SchemaName.TrimSafe();

			var tablePerSubclass = type.FirstAttribute<TablePerSubclassAttribute>(true);
			if (tablePerSubclass != null)
			{
				tablePerSubclass = type.FirstAttribute<TablePerSubclassAttribute>(false);
				if (tablePerSubclass == null)
					return SchemaName(type.BaseType);
				if (!tablePerSubclass.SchemaName.IsNullOrWhiteSpace())
					schema = tablePerSubclass.SchemaName.TrimSafe();
			}
			return schema;
		}

		public static string TableName(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			var table = type.Name;

			var tableAttribute = type.FirstAttribute<TableAttribute>(false);
			if (tableAttribute != null && !tableAttribute.TableName.IsNullOrWhiteSpace())
				table = tableAttribute.TableName.TrimSafe();

			var tablePerSubclass = type.FirstAttribute<TablePerSubclassAttribute>(true);
			if(tablePerSubclass!=null)
			{
				tablePerSubclass = type.FirstAttribute<TablePerSubclassAttribute>(false);
				if (tablePerSubclass == null)
					return TableName(type.BaseType);
				if (!tablePerSubclass.TableName.IsNullOrWhiteSpace())
					table = tablePerSubclass.TableName.TrimSafe();
			}

			return table;
		}

		public static string ColumnName<TModel>(Expression<Func<TModel, object>> expression)
		{
			var propertyInfo = expression.PropertyInfoFromExpression();
			var name = propertyInfo.Name;
			if (propertyInfo.PropertyType.IsClass && !typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
				name += "Id";
			return name;
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax ColumnOptions<TModel>(
			this ICreateTableColumnOptionOrWithColumnSyntax self, PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				return self.Nullable();
			var tablePerSubclass = typeof(TModel).FirstAttribute<TablePerSubclassAttribute>(true);
			if (tablePerSubclass != null)
			{
				tablePerSubclass = typeof(TModel).FirstAttribute<TablePerSubclassAttribute>(false);
				if (tablePerSubclass == null)
					return self.Nullable();
			} 
			var notEmpty = propertyInfo.FirstAttribute<NotEmptyAttribute>(true);
			var notNull = propertyInfo.FirstAttribute<NotNullAttribute>(true);
			var result = notEmpty != null || notNull!=null
				? self.NotNullable()
				: self.Nullable();
			return result;
		}

		public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax ColumnOptions<TModel>(
			this IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax self, PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				return self.Nullable();
			var tablePerSubclass = typeof(TModel).FirstAttribute<TablePerSubclassAttribute>(true);
			if (tablePerSubclass != null)
			{
				tablePerSubclass = typeof(TModel).FirstAttribute<TablePerSubclassAttribute>(false);
				if (tablePerSubclass == null)
					return self.Nullable();
			} 
			var notEmpty = propertyInfo.FirstAttribute<NotEmptyAttribute>(true);
			var notNull = propertyInfo.FirstAttribute<NotNullAttribute>(true);
			var result = notEmpty != null || notNull!=null
				? self.NotNullable()
				: self.Nullable();
			return result;
		}
		
		#endregion
	} 
}
