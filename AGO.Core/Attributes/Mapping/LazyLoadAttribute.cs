using System;

namespace AGO.Core.Attributes.Mapping
{
	/// <summary>
	/// Определяет необходимость отложенной загрузки значения свойства
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class LazyLoadAttribute: Attribute
	{
	}
}