using System;

namespace AGO.Core.Attributes.Constraints
{
	/// <summary>
	/// Помечает параметры или свойства, которые должны входить в определенный диапазон
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true)]
	public class InRangeAttribute: Attribute
	{
		public object Start { get; private set; }
		public object End { get; private set; }
		public bool Inclusive { get; set; }

		public InRangeAttribute(object start, object end, bool inclusive = true)
		{
			Start = start;
			End = end;
			Inclusive = inclusive;
		}
	}
}
