using System;

namespace AGO.Hibernate.Attributes.Model
{
	/// <summary>
	/// Помечает свойства моделей которые должны подгружаться только при первом обращении 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PrefetchedAttribute: Attribute
	{
	}
}