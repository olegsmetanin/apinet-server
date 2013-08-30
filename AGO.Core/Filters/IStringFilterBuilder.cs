using AGO.Core.Model;

namespace AGO.Core.Filters
{
	public interface IStringFilterBuilder<TModel, TPrevModel> : IValueFilterNode
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		IModelFilterBuilder<TModel, TPrevModel> Eq(string str);

		IModelFilterBuilder<TModel, TPrevModel> IsNull();

		IModelFilterBuilder<TModel, TPrevModel> Like(
			string str, 
			bool prependWildcard = false, 
			bool appendWildcard = false);

		IStringFilterBuilder<TModel, TPrevModel> Not();
	}
}