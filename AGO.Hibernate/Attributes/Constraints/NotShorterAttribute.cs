using System;

namespace AGO.Hibernate.Attributes.Constraints
{
	/// <summary>
	/// Помечает параметры или свойства, которые не должны иметь длину меньше чем заданное значение (для тех к кому применимо) 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
	public class NotShorterAttribute : InRangeAttribute
	{
		public NotShorterAttribute(int limit)
			: base(limit, null, true)
		{
		}

		public int Limit { get { return (int) Start; } }
	}
}