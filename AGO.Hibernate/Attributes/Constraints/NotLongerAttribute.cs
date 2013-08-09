using System;

namespace AGO.Hibernate.Attributes.Constraints
{
	/// <summary>
	/// Помечает параметры или свойства, которые не должны иметь длину больше чем заданное значение (для тех к кому применимо) 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
	public class NotLongerAttribute: InRangeAttribute
	{
		public NotLongerAttribute(int limit)
			:base(null, limit, true)
		{
		}

		public int Limit { get { return (int) End; } }
	}
}