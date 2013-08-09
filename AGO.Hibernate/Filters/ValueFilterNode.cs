using System.Collections.Generic;
using AGO.Hibernate.Json;
using Newtonsoft.Json;

namespace AGO.Hibernate.Filters
{
	[JsonObject(MemberSerialization.OptIn)]
	internal class ValueFilterNode : AbstractFilterNode, IValueFilterNode
	{
		public static IDictionary<ValueFilterOperators, string> OperatorConversionTable = new Dictionary<ValueFilterOperators, string>
		{
			{ ValueFilterOperators.Exists, "exists" },

			{ ValueFilterOperators.Eq, "=" },	
			{ ValueFilterOperators.Like, "like" },
			{ ValueFilterOperators.Lt, "<" },
			{ ValueFilterOperators.Gt, ">" },
			{ ValueFilterOperators.Le, "<=" },
			{ ValueFilterOperators.Ge, ">=" }
		};

		[JsonProperty(PropertyName = "op"), JsonConverter(typeof(ValueFilterOperatorConverter))]
		public ValueFilterOperators? Operator { get; set; }

		[JsonProperty(PropertyName = "value")]
		public string Operand { get; set; }

		public bool IsUnary
		{
			get { return Operator == ValueFilterOperators.Exists; }
		}

		public bool IsBinary
		{
			get 
			{ 
				return Operator == ValueFilterOperators.Eq ||
					Operator == ValueFilterOperators.Like || 
					Operator == ValueFilterOperators.Lt ||
					Operator == ValueFilterOperators.Gt ||
					Operator == ValueFilterOperators.Le ||
					Operator == ValueFilterOperators.Ge; 
			}
		}

		public bool IsTernary { get { return !IsUnary && !IsBinary; } }
	}
}