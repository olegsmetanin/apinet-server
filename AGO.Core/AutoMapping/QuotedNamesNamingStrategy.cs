using System;
using NHibernate.Cfg;

namespace AGO.Core.AutoMapping
{
	/// <summary>
	/// Проблема вылезла когда переходили с MS Sql Server на PostgreSql. Оказывается, там 
	/// если имя без кавычек и имеет разные регистры, то оно приводится к lowercase и postgre
	/// просто не может найти схему/таблицу/колонку. Пришлось добавить такую обработку всех имен.
	/// Взято отсюда: http://manfredlange.blogspot.ru/2011/04/fluent-nhibernate-postgresql-and.html
	/// Работать по идее должно везде, т.к. quoted identifiers это стандарт, и, по крайне мере в ms sql это настраивается
	/// (SET QUOTED_IDENTIFIERS ON)
	/// </summary>
	/// <remarks>
	/// Т.к. этот класс не решает вопрос с квотингом имени схемы, то пришлось дополнительно пофиксить <see cref="ClassConvention"/>
	/// в строке 31 (где задается имя схемы для класса) и PersistentCollectionConvention тоже (для случая М:М через таблицу, без сущности)
	/// </remarks>
	internal class QuotedNamesNamingStrategy : INamingStrategy
	{
		public string ClassToTableName(string className)
		{
			return DoubleQuote(className);
		}
		public string PropertyToColumnName(string propertyName)
		{
			return DoubleQuote(propertyName);
		}
		public string TableName(string tableName)
		{
			return DoubleQuote(tableName);
		}
		public string ColumnName(string columnName)
		{
			return DoubleQuote(columnName);
		}
		public string PropertyToTableName(string className,
											string propertyName)
		{
			return DoubleQuote(propertyName);
		}
		public string LogicalColumnName(string columnName,
										string propertyName)
		{
			return String.IsNullOrWhiteSpace(columnName) ?
				DoubleQuote(propertyName) :
				DoubleQuote(columnName);
		}
		public static string DoubleQuote(string raw)
		{
			// In some cases the identifier is single-quoted.
			// We simply remove the single quotes:
			raw = raw.Replace("`", "");
			return String.Concat("\"", raw, "\"");
		}
	}
}