using System;

namespace AGO.Hibernate.Attributes.Model
{
	/// <summary>
	/// Помечает свойство модели, в котором хранится ее версия (для конкурентного доступа) 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ModelVersionAttribute : Attribute
	{
	}
}
