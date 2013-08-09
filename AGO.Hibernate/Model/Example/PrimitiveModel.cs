using System;
using System.ComponentModel;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Hibernate.Model.Example
{
	public class PrimitiveModel : CommonModel<Guid>
	{
		[DisplayName("Свойство типа String"), NotLonger(64), JsonProperty]
		public virtual string StringProperty { get; set; }

		[DisplayName("Свойство типа Guid"), JsonProperty]
		public virtual Guid GuidProperty { get; set; }

		[DisplayName("Свойство типа DateTime"), JsonProperty]
		public virtual DateTime DateTimeProperty { get; set; }

		[DisplayName("Свойство типа Enum"), JsonProperty, EnumDisplayNames(new[]
		{
			"Value1", "Значение 1",
			"Value2", "Значение 2"
		})]
		public virtual ExampleEnum EnumProperty { get; set; }

		[DisplayName("Свойство типа Bool"), JsonProperty]
		public virtual bool BoolProperty { get; set; }

		[DisplayName("Свойство типа Byte"), JsonProperty]
		public virtual byte ByteProperty { get; set; }

		[DisplayName("Свойство типа Char"), JsonProperty]
		public virtual char CharProperty { get; set; }

		[DisplayName("Свойство типа Decimal"), JsonProperty]
		public virtual decimal DecimalProperty { get; set; }

		[DisplayName("Свойство типа Double"), JsonProperty]
		public virtual double DoubleProperty { get; set; }

		[DisplayName("Свойство типа Float"), JsonProperty]
		public virtual float FloatProperty { get; set; }

		[DisplayName("Свойство типа Int"), JsonProperty]
		public virtual int IntProperty { get; set; }

		[DisplayName("Свойство типа Long"), JsonProperty]
		public virtual long LongProperty { get; set; }

		[DisplayName("Nullable свойство типа Guid"), JsonProperty]
		public virtual Guid? NullableGuidProperty { get; set; }

		[DisplayName("Nullable свойство типа DateTime"), JsonProperty]
		public virtual DateTime? NullableDateTimeProperty { get; set; }

		[DisplayName("Nullable свойство типа Enum"), JsonProperty, EnumDisplayNames(new[]
		{
			"Value1", "Значение 1",
			"Value2", "Значение 2"
		})]
		public virtual ExampleEnum? NullableEnumProperty { get; set; }

		[DisplayName("Nullable свойство типа Bool"), JsonProperty]
		public virtual bool? NullableBoolProperty { get; set; }

		[DisplayName("Nullable свойство типа Byte"), JsonProperty]
		public virtual byte? NullableByteProperty { get; set; }

		[DisplayName("Nullable свойство типа Char"), JsonProperty]
		public virtual char? NullableCharProperty { get; set; }

		[DisplayName("Nullable свойство типа Decimal"), JsonProperty]
		public virtual decimal? NullableDecimalProperty { get; set; }

		[DisplayName("Nullable свойство типа Double"), JsonProperty]
		public virtual double? NullableDoubleProperty { get; set; }

		[DisplayName("Nullable свойство типа Float"), JsonProperty]
		public virtual float? NullableFloatProperty { get; set; }

		[DisplayName("Nullable свойство типа Int"), JsonProperty]
		public virtual int? NullableIntProperty { get; set; }

		[DisplayName("Nullable свойство типа Long"), JsonProperty]
		public virtual long? NullableLongProperty { get; set; }

		[DisplayName("Ссылка на ManyEndModel")]
		public virtual ManyEndModel ManyEnd { get; set; }
	}
}