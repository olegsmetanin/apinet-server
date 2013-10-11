namespace AGO.Core.Model.Processing
{
	public interface IModelValidator
	{
		int Priority { get; }

		bool Accepts(IIdentifiedModel model);

		void ValidateModelSaving(IIdentifiedModel model, ValidationResult validation, object capability = null);

		void ValidateModelDeletion(IIdentifiedModel model, ValidationResult validation, object capability = null);
	}
}
