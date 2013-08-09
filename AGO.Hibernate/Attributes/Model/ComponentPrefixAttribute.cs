using System;

namespace AGO.Hibernate.Attributes.Model
{
	/// <summary>
	/// Классы компоненты группируют свойства моделей, для разбития на логические группы. 
	/// Атрибует определяет каким префиксом будет дополнено название каждого свойства компонента 
	/// в сохраненной модели (по умолчанию без префикса)
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ComponentPrefixAttribute: Attribute
	{
		public ComponentPrefixAttribute(string prefix)
		{
			Prefix = prefix;
		}

		public string Prefix { get; private set; }
	}
}