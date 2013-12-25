using System;
using AGO.Core.Attributes.Mapping;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Nullables;

namespace AGO.Core.AutoMapping
{
	public class PropertyConvention : IPropertyConvention
	{
		public void Apply(IPropertyInstance instance)
		{
			var readOnly =
				instance.Property.MemberInfo.FirstAttribute<ReadOnlyPropertyAttribute>(true);
			if (readOnly != null)
				instance.ReadOnly();

			//not really work at all in ms sql
//			var lazy = instance.Property.MemberInfo.FirstAttribute<LazyLoadAttribute>(true);
//			if (lazy != null)
//				instance.LazyLoad();

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
