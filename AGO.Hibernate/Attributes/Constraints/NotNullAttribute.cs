using System;

namespace AGO.Hibernate.Attributes.Constraints
{
	/// <summary>
	/// Помечает параметры или свойства, которые не должны быть null
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
	public class NotNullAttribute : Attribute
	{
	}
}