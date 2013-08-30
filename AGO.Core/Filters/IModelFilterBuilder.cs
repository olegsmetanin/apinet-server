using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	public interface IModelFilterBuilder<TModel, TPrevModel> : IModelFilterNode
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		IModelFilterBuilder<TModel, TPrevModel> And();

		IModelFilterBuilder<TModel, TPrevModel> Or();

		IModelFilterBuilder<TNextModel, TModel> WhereModel<TNextModel>(
			Expression<Func<TModel, TNextModel>> expression)
			where TNextModel : class, IIdentifiedModel;

		IModelFilterBuilder<TNextModel, TModel> WhereCollection<TNextModel>(
			Expression<Func<TModel, IEnumerable<TNextModel>>> expression)
			where TNextModel : class, IIdentifiedModel;

		IModelFilterBuilder<TPrevModel, TPrevModel> End();

		IModelFilterBuilder<TModel, TPrevModel> Where<TValue>(
			Expression<Func<TModel, TValue>> expression)
			where TValue : struct;

		IValueFilterBuilder<TModel, TPrevModel, TValue> WhereProperty<TValue>(
			Expression<Func<TModel, TValue>> expression);

		IStringFilterBuilder<TModel, TPrevModel> WhereString(
			Expression<Func<TModel, string>> expression);
	}
}