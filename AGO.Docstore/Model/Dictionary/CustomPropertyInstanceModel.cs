using System;
using System.ComponentModel;
using System.Globalization;
using AGO.Docstore.Model.Security;
using AGO.Hibernate;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Dictionary
{
	[TablePerSubclass("ModelType")]
	public abstract class CustomPropertyInstanceModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Тип параметра"), JsonProperty, NotNull]
		public virtual CustomPropertyTypeModel PropertyType { get; set; }
		[ReadOnlyProperty]
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
		
		#endregion
	}
}
