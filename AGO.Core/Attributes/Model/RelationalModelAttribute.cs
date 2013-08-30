using System;

namespace AGO.Core.Attributes.Model
{
	/// <summary>
	/// Помечает модель как реляционную, такие модели могут мапиться на БД имеющие схему данных.
	/// Не запрещает использовать такие модели одноверменно и в NoSQL
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class RelationalModelAttribute : Attribute
	{
	}
}
