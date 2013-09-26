namespace AGO.Core.Model.Processing
{
	public interface IModelValidator
	{
		int Priority { get; }

		bool Accepts(IIdentifiedModel model);

		void ValidateModel(IIdentifiedModel model, ValidationResult validation, object capability = null);
	}
}
