using System;

namespace AGO.Hibernate.Attributes.Model
{
	/// <summary>
	/// Помечает одно или несколько (составные идентификаторы) свойств  как уникально идентифицирующие модель 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IdentifierAttribute: Attribute
	{
	}
}