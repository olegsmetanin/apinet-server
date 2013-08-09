using AGO.Hibernate.Model;

namespace AGO.Hibernate.Filters
{
	public interface IValueFilterBuilder<TModel, TPrevModel, in TValue> : IValueFilterNode
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		IModelFilterBuilder<TModel, TPrevModel> Exists();

		IModelFilterBuilder<TModel, TPrevModel> Eq(TValue val);

		IModelFilterBuilder<TModel, TPrevModel> Lt(TValue val);

		IModelFilterBuilder<TModel, TPrevModel> Gt(TValue val);

		IModelFilterBuilder<TModel, TPrevModel> Le(TValue val);

		IModelFilterBuilder<TModel, TPrevModel> Ge(TValue val);

		IValueFilterBuilder<TModel, TPrevModel, TValue> Not();
	}
}