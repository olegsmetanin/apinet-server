using System;
using System.Linq;
using AGO.Hibernate.Filters;
using Newtonsoft.Json;

namespace AGO.Hibernate.Json
{
	public class ValueFilterOperatorConverter : JsonConverter
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var strVal = reader.Value.ToStringSafe().Trim();
			
			var exact = strVal.ParseEnumSafe<ValueFilterOperators>();
			if (exact != null)
				return exact;

			foreach (var pair in ValueFilterNode.OperatorConversionTable.Where(
					pair => pair.Value.Equals(strVal, StringComparison.InvariantCultureIgnoreCase)))
				return pair.Key;
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
				return;

			var op = value as ValueFilterOperators?;
			if (op == null)
				throw new ArgumentException("ValueFilterOperators enum expected");

			writer.WriteValue(ValueFilterNode.OperatorConversionTable[(ValueFilterOperators) op]);
		}

		public override bool CanConvert(Type objectType)
		{
			var realType = objectType;
			if (objectType.IsNullable())
				realType = objectType.GetGenericArguments()[0];

			return typeof(ValueFilterOperators).IsAssignableFrom(realType);
		}
	}
}
