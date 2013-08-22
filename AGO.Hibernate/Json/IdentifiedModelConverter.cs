using System;
using System.Linq;
using System.Reflection;
using AGO.Hibernate.Model;
using NHibernate.Proxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Hibernate.Json
{
	public class IdentifiedModelConverter : JsonConverter
	{
		#region Constants

		public const string ModelTypePropertyName = "ModelType";

		public const string ModelAssemblyPropertyName = "ModelAssembly";
		
		#endregion

		#region Abstract methods implementation

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = (JObject)JToken.ReadFrom(reader);

			var modelType = obj.ModelType();
			if (modelType == null)
				throw new InvalidModelTypeInJsonException();

			var model = (IIdentifiedModel)Activator.CreateInstance(modelType);
			foreach (var property in obj.Properties().Where(
					p => !ModelTypePropertyName.Equals(p.Name, StringComparison.InvariantCulture)))
				ReadProperty(model, modelType, property, serializer);

			return model;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(IIdentifiedModel).IsAssignableFrom(objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var model = (IIdentifiedModel) value;

			writer.WriteStartObject();

			foreach (var propertyInfo in model.RealType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
			{
				var attributes = propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
				if (attributes.Length == 0)
					continue;

				WriteProperty(model, propertyInfo, writer, serializer, (JsonPropertyAttribute) attributes[0]);
			}

			/*writer.WritePropertyName(ModelTypePropertyName);
			writer.WriteValue(model.RealType.FullName);
			writer.WritePropertyName(ModelAssemblyPropertyName);
			writer.WriteValue(model.RealType.Assembly.GetName().Name);*/

			writer.WriteEndObject();
		} 

		#endregion

		#region Helper methods

		protected void WriteProperty(
			IIdentifiedModel model,
			PropertyInfo propertyInfo,
			JsonWriter writer,
			JsonSerializer serializer,
			JsonPropertyAttribute attribute)
		{
			var propertyName = attribute == null || attribute.PropertyName.IsNullOrWhiteSpace()
				? propertyInfo.Name
				: attribute.PropertyName;
			var value = propertyInfo.GetValue(model, null);

			if (typeof(IIdentifiedModel).IsAssignableFrom(propertyInfo.PropertyType) &&
				value.IsProxy() && ((INHibernateProxy) value).HibernateLazyInitializer.IsUninitialized)
			{
				var modelIdProperty = model.GetType().GetProperty(
					propertyInfo.Name + "Id", BindingFlags.Public | BindingFlags.Instance);
				if (modelIdProperty == null)
					return;
				
				writer.WritePropertyName(propertyName);
				writer.WriteStartObject();
				writer.WritePropertyName("Id");
				serializer.Serialize(writer, modelIdProperty.GetValue(model, null));
				writer.WriteEndObject();
				return;
			}
			
			writer.WritePropertyName(propertyName);
			serializer.Serialize(writer, value);		
		}

		protected void ReadProperty(IIdentifiedModel model, Type modelType, JProperty property, JsonSerializer serializer)
		{
			PropertyInfo modelProperty = null;
			foreach (var propertyInfo in model.RealType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
			{
				var attributes = propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
				if (attributes.Length == 0)
					continue;
				var attribute = (JsonPropertyAttribute)attributes[0];

				var propertyName = attribute.PropertyName.IsNullOrWhiteSpace()
					? propertyInfo.Name
					: attribute.PropertyName;

				if (!propertyName.Equals(property.Name, StringComparison.InvariantCulture))
					continue;

				modelProperty = propertyInfo;
				break;
			}

			if (modelProperty == null)
				return;

			var tokenReader = new JTokenReader(property.Value) {CloseInput = false};
			var value = serializer.Deserialize(tokenReader, modelProperty.PropertyType);

			var dateTime = value as DateTime?;
			if (dateTime != null && dateTime.Value.Kind == DateTimeKind.Unspecified)
				value = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Local);

			var component = value as IComponent;
			if (component != null)
				ProcessComponent(component, property.Value as JObject, modelProperty.Name);

			modelProperty.SetValue(model, value, null);
		}

		protected void ProcessComponent(IComponent component, JObject obj, string prefix)
		{
			if (component == null || obj == null)
				return;

			var properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(pi => pi.CanRead && pi.CanWrite).ToArray();

			foreach (var propertyInfo in properties)
			{
				var isNullableEnum = false;
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsNullable())
				{
					propertyType = propertyType.GetGenericArguments()[0];
					if (propertyType.IsEnum)
						isNullableEnum = true;
				}
				if (!propertyType.IsValueType && !typeof(string).IsAssignableFrom(propertyType))
					continue;

				var otherProperty = obj.Property(prefix + propertyInfo.Name);
				if (otherProperty == null)
					continue;
				var jValue = otherProperty.Value as JValue;
				if (jValue == null)
					continue;
				if (jValue.Value == null && !isNullableEnum)
					continue;
				propertyInfo.SetValue(component, jValue.Value.ConvertSafe(propertyInfo.PropertyType), null);
			}
		}

		#endregion
	}
}