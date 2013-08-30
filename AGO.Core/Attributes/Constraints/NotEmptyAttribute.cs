using System;

namespace AGO.Core.Attributes.Constraints
{
	/// <summary>
	/// Помечает параметры или свойства, которые не должны быть пустыми (или иметь значение по умолчанию)
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
	public class NotEmptyAttribute : Attribute
	{
		public bool IgnoreWhitespace { get; set; }

		public NotEmptyAttribute()
		{
			IgnoreWhitespace = true;
		}
	}
}