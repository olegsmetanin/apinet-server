using System;
using System.ComponentModel;
using System.Globalization;
using AGO.Core.Model.Security;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	[TablePerSubclass("ModelType")]
	public abstract class CustomPropertyInstanceModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Тип параметра"), JsonProperty, NotNull]
		public virtual CustomPropertyTypeModel PropertyType { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? PropertyTypeId { get; set; }

		[DisplayName("Значение-строка"), JsonProperty, NotLonger(512)]
		public virtual string StringValue { get; set; }

		[DisplayName("Значение-число"), JsonProperty]
		public virtual Decimal? NumberValue { get; set; }

		[DisplayName("Значение-дата"), JsonProperty]
		public virtual DateTime? DateValue { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			var value = StringValue;
			if (NumberValue != null)
				value = NumberValue.Value.ToString(CultureInfo.CurrentCulture);
			if (DateValue != null)
				value = DateValue.Value.ToLocalTime().ToShortDateString();

			return string.Format("{0} - {1}", PropertyType.ToStringSafe(), value);
		}

		[NotMapped]
		public virtual object Value
		{
			get
			{
				if (PropertyType == null)
					throw new InvalidOperationException("Can not read parameter value without type");
				switch (PropertyType.ValueType)
				{
					case CustomPropertyValueType.String:
						return StringValue;
					case CustomPropertyValueType.Number:
						return NumberValue;
					case CustomPropertyValueType.Date:
						return DateValue;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			set
			{
				if (PropertyType == null)
					throw new InvalidOperationException("Can not read parameter value without type");
				switch (PropertyType.ValueType)
				{
					case CustomPropertyValueType.String:
						StringValue = value.ConvertSafe<string>();
						break;
					case CustomPropertyValueType.Number:
						NumberValue = value.ConvertSafe<decimal?>();
						break;
					case CustomPropertyValueType.Date:
						DateValue = value.ConvertSafe<DateTime?>();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		
		#endregion
	}
}
