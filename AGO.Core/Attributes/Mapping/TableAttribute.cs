using System;

namespace AGO.Core.Attributes.Mapping
{
	/// <summary>
	/// Определяет отличное от имени класса имя таблицы и позволяет указать схему таблицы
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TableAttribute: Attribute
	{
		public string TableName { get; set; }

		public string SchemaName { get; set; }
	}
}