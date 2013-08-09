using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Model;
using AGO.Hibernate.Nullables;

namespace AGO.Hibernate.AutoMapping
{
	public class PropertyConvention : IPropertyConvention
	{
		public void Apply(IPropertyInstance instance)
		{
			var readOnly =
				instance.Property.MemberInfo.FirstAttribute<ReadOnlyAttribute>(true);
			if (readOnly != null)
				instance.ReadOnly();

			var propertyType = instance.Property.PropertyType;
			if (propertyType.IsNullable())
			{
				var genericArguments = propertyType.GetGenericArguments();
				propertyType = genericArguments.Length > 0 ? genericArguments[0] : null;
				if (typeof(DateTime).IsAssignableFrom(propertyType))
					instance.CustomType<NullableDateTime>();
			}

			if (!typeof(string).IsAssignableFrom(propertyType))
				return;
			var notLonger = instance.Property.MemberInfo.FirstAttribute<NotLongerAttribute>(true);
			if (notLonger != null)
				instance.Length(notLonger.Limit);
			else
			{
				instance.CustomSqlType("StringClobSqlType");
				instance.Length(int.MaxValue);
			}
		}
	}
}
