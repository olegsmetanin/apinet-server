using System;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
using AGO.Core;


namespace AGO.Tasks.Controllers
{
	public static class Extensions
	{
		public static string EnumDisplayValue<TModel, TEnum>(
			this IModelMetadata meta, Expression<Func<TModel, TEnum>> property, TEnum value)
		{
			var name = property.PropertyInfoFromExpression().Name;
			var prop = meta.PrimitiveProperties.FirstOrDefault(p => p.Name == name);
			if (prop == null)
				throw new ArgumentException("Invalid property for retrive enum display value", "property");

			return prop.PossibleValues[value.ToString()];
		}

		public static LookupEntry EnumLookupEntry<TModel, TEnum>(
			this IModelMetadata meta, Expression<Func<TModel, TEnum>> property, TEnum value)
		{
			var name = property.PropertyInfoFromExpression().Name;
			var prop = meta.PrimitiveProperties.FirstOrDefault(p => p.Name == name);
			if (prop == null)
				throw new ArgumentException("Invalid property for retrive enum display value", "property");

			return new LookupEntry {Id = value.ToString(), Text = prop.PossibleValues[value.ToString()]};
		}
	}
}