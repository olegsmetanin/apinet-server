namespace AGO.Core.Model.Processing
{
	public interface IModelPostProcessor
	{
		bool Accepts(IIdentifiedModel model);

		void AfterModelCreated(IIdentifiedModel model);

		void AfterModelUpdated(IIdentifiedModel model, IIdentifiedModel original);

		void AfterModelDeleted(IIdentifiedModel model);
	}
}
