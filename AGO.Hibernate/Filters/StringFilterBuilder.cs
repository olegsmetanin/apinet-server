using System;
using System.Reflection;
using AGO.Hibernate.Model;

namespace AGO.Hibernate.Filters
{
	internal class StringFilterBuilder<TModel, TPrevModel> : ValueFilterNode, IStringFilterBuilder<TModel, TPrevModel>
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		#region Properties, fields, constructors

		protected readonly PropertyInfo _PropertyInfo;

		protected readonly ModelFilterBuilder<TModel, TPrevModel> _ModelFilterBuilder;

		public StringFilterBuilder(PropertyInfo propertyInfo, ModelFilterBuilder<TModel, TPrevModel> modelFilterBuilder)
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

		public IModelFilterBuilder<TModel, TPrevModel> Eq(string str)
		{
			Operator = ValueFilterOperators.Eq;
			Operand = str.TrimSafe();
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> IsNull()
		{
			Operator = ValueFilterOperators.Exists;
			return _ModelFilterBuilder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Like(
			string str,
			bool prependWildcard = false,
			bool appendWildcard = false)
		{
			Operator = ValueFilterOperators.Like;

			str = str.TrimSafe();
			str = prependWildcard ? str.AddPrefix("%") : str;
			str = appendWildcard ? str.AddSuffix("%") : str;

			Operand = str;
			return _ModelFilterBuilder;
		}

		public IStringFilterBuilder<TModel, TPrevModel> Not()
		{
			Negative = !Negative;
			return this;
		}

		#endregion		
	}
}