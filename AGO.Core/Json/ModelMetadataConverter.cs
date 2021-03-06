﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using AGO.Core.Application;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Filters.Metadata;

namespace AGO.Core.Json
{
	public class ModelMetadataConverter : JsonConverter
	{
		protected ILocalizationService _LocalizationService;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(IEnumerable<IModelMetadata>).IsAssignableFrom(objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var metaCollection = value as IEnumerable<IModelMetadata> ?? Enumerable.Empty<IModelMetadata>();

			writer.WriteStartObject();

			foreach (var modelMeta in metaCollection)
				WriteModelMetaData(writer, modelMeta, serializer);

			writer.WriteEndObject();
		}

		protected void WriteModelMetaData(
			JsonWriter writer,
			IModelMetadata modelMeta,
			JsonSerializer serializer)
		{
			writer.WritePropertyName(modelMeta.Name);

			writer.WriteStartObject();

			writer.WritePropertyName("PrimitiveProperties");
			writer.WriteStartObject();
			foreach (var propertyMeta in modelMeta.PrimitiveProperties)
				WritePropertyMetaData(writer, modelMeta, propertyMeta, serializer);
			writer.WriteEndObject();

			writer.WritePropertyName("ModelProperties");
			writer.WriteStartObject();
			foreach (var propertyMeta in modelMeta.ModelProperties)
				WritePropertyMetaData(writer, modelMeta, propertyMeta, serializer);
			writer.WriteEndObject();

			writer.WriteEndObject();
		}

		protected void WritePropertyMetaData(JsonWriter writer, 
			IModelMetadata modelMeta, 
			IPropertyMetadata propertyMeta, 
			JsonSerializer serializer)
		{
			writer.WritePropertyName(propertyMeta.Name);
			writer.WriteStartObject();

			var displayName = propertyMeta.Name;

			_LocalizationService = _LocalizationService ?? AbstractApplication.Current.LocalizationService;
			if (_LocalizationService != null)
			{
				var localized = _LocalizationService.MessageForType(modelMeta.ModelType, propertyMeta.Name);
				if (localized.IsNullOrWhiteSpace())
				{
					var currentType = modelMeta.ModelType;
					while (currentType!=null && typeof(ICoreModel).IsAssignableFrom(currentType))
					{
						localized = _LocalizationService.MessageForType(typeof(ICoreModel), 
							string.Format("{0}.{1}", currentType.Name.RemoveSuffix("`1"), propertyMeta.Name));
						if (!localized.IsNullOrWhiteSpace())
							break;
						currentType = currentType.BaseType;
					}
				}

				displayName = !localized.IsNullOrWhiteSpace() ? localized.TrimSafe() : displayName;
			}

			writer.WritePropertyName("DisplayName");
			writer.WriteValue(displayName);

			var modelPropertyMeta = propertyMeta as IModelPropertyMetadata;
			if (modelPropertyMeta != null)
				WriteModelPropertyMetaData(writer, modelPropertyMeta, serializer);

			var primitivePropertyMeta = propertyMeta as IPrimitivePropertyMetadata;
			if (primitivePropertyMeta != null)
				WritePrimitivePropertyMetaData(writer, primitivePropertyMeta, serializer);

			writer.WriteEndObject();
		}

		protected void WriteModelPropertyMetaData(JsonWriter writer, IModelPropertyMetadata propertyMeta, JsonSerializer serializer)
		{
			writer.WritePropertyName("IsCollection");
			writer.WriteValue(propertyMeta.IsCollection);

			writer.WritePropertyName("ModelType");
			writer.WriteValue(propertyMeta.PropertyType.FullName);
		}

		protected void WritePrimitivePropertyMetaData(JsonWriter writer, IPrimitivePropertyMetadata propertyMeta, JsonSerializer serializer)
		{
			writer.WritePropertyName("PropertyType");

			var propertyType = propertyMeta.PropertyType;
			if (typeof(Guid) == propertyType)
				writer.WriteValue("guid");
			else if (typeof(DateTime) == propertyType)
				writer.WriteValue(propertyMeta.IsTimestamp ? "datetime" : "date");
			else if (typeof(bool) == propertyType)
				writer.WriteValue("boolean");
			else if (propertyType.IsEnum)
			{
				writer.WriteValue("enum");

				writer.WritePropertyName("PossibleValues");
				var valuesMap = new Dictionary<string, string>();
				foreach (var name in Enum.GetNames(propertyType))
				{
					var localizedName = _LocalizationService.MessageForType(propertyType, name);
					valuesMap[name] = (localizedName ?? name).TrimSafe();
				}

				serializer.Serialize(writer, valuesMap);
			}
			else if (typeof(byte) == propertyType || typeof(sbyte) == propertyType)
				writer.WriteValue("int");
			else if (typeof(char) == propertyType)
				writer.WriteValue("string");
			else if (typeof(Decimal) == propertyType)
				writer.WriteValue("float");
			else if (typeof(float) == propertyType)
				writer.WriteValue("float");
			else if (typeof(double) == propertyType)
				writer.WriteValue("float");
			else if (typeof(short) == propertyType || typeof(ushort) == propertyType)
				writer.WriteValue("int");
			else if (typeof(int) == propertyType || typeof(uint) == propertyType)
				writer.WriteValue("int");
			else if (typeof(long) == propertyType || typeof(ulong) == propertyType)
				writer.WriteValue("int");
			else if (typeof (string) == propertyType)
				writer.WriteValue("string");
			else
				throw new InvalidOperationException("Unexpected property type");
		}
	}
}
