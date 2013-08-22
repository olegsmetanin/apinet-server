using System;
using System.Linq;
using Newtonsoft.Json;
using AGO.Hibernate.Filters;

namespace AGO.Hibernate.Json
{
	public class ModelFilterOperatorConverter : JsonConverter
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var strVal = reader.Value.ToStringSafe().Trim();
			
			var exact = strVal.ParseEnumSafe<ModelFilterOperators>();
			if (exact != null)
				return exact;

			foreach (var pair in ModelFilterNode.OperatorConversionTable.Where(
					pair => pair.Value.Equals(strVal, StringComparison.InvariantCultureIgnoreCase)))
				return pair.Key;
			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
				return;

			var op = value as ModelFilterOperators?;
			if (op == null)
				throw new ArgumentException("ModelFilterOperators enum expected");

			writer.WriteValue(ModelFilterNode.OperatorConversionTable[(ModelFilterOperators) op]);
		}

		public override bool CanConvert(Type objectType)
		{
			var realType = objectType;
			if (objectType.IsNullable())
				realType = objectType.GetGenericArguments()[0];

			return typeof(ModelFilterOperators).IsAssignableFrom(realType);
		}
	}
}
