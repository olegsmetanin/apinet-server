using System;

namespace AGO.Hibernate.Attributes.Mapping
{
	/// <summary>
	/// Определяет будут ли наследованные модели храниться в одной таблице с помеченной этим атрибутом,
	/// и в каком столбце будет записана информация о конкретном типе модели
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TablePerSubclassAttribute : Attribute
	{
		public string TableName { get; set; }

		public string DiscriminatorColumn { get; private set; }

		public TablePerSubclassAttribute(string discriminatorColumn)
		{
			if(discriminatorColumn.IsNullOrEmpty())
				throw new ArgumentNullException("discriminatorColumn");
			DiscriminatorColumn = discriminatorColumn;
		}
	}
}
