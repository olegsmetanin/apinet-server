using System;
using System.Reflection;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	internal class ValueFilterBuilder<TModel, TPrevModel, TValue> : ValueFilterNode, IValueFilterBuilder<TModel, TPrevModel, TValue>
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		#region Properties, fields, constructors

		protected readonly PropertyInfo _PropertyInfo;

		protected readonly ModelFilterBuilder<TModel, TPrevModel> _ModelFilterBuilder;

		public ValueFilterBuilder(PropertyInfo propertyInfo, ModelFilterBuilder<TModel, TPrevModel> modelFilterBuilder)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			Path = propertyInfo.Name;
			_PropertyInfo = propertyInfo;

			if (modelFilterBuilder == null)
				throw new ArgumentNullException("modelFilterBuilder");
			_ModelFilterBuilder = modelFilterBuilder;
		}

		#endregion

		#region Interfaces implementation

		public IModelFilterBuilder<TModel, TPrevModel> Exists()
		{
			Operator = ValueFilterOperators.Exists;
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Eq(TValue val)
		{
			Operator = ValueFilterOperators.Eq;
			Operand = val.ToString();
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Lt(TValue val)
		{
			Operator = ValueFilterOperators.Lt;
			Operand = val.ToString();
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Gt(TValue val)
		{
			Operator = ValueFilterOperators.Gt;
			Operand = val.ToString();
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Le(TValue val)
		{
			Operator = ValueFilterOperators.Le;
			Operand = val.ToString();
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Ge(TValue val)
		{
			Operator = ValueFilterOperators.Ge;
			Operand = val.ToString();
			return _ModelFilterBuilder;
		}

		public IValueFilterBuilder<TModel, TPrevModel, TValue> Not()
		{
			Negative = !Negative;
			return this;
		}

		#endregion
	}
}