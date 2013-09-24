using AGO.Core.Model;

namespace AGO.Core.Validation
{
	public interface IValidationService
	{
		void RegisterModelValidator(IModelValidator validator);

		void ValidateModel(IIdentifiedModel model, ValidationResult validation);
	}
}
