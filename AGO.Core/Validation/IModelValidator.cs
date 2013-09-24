using AGO.Core.Model;

namespace AGO.Core.Validation
{
	public interface IModelValidator
	{
		int Priority { get; }

		bool Accepts(IIdentifiedModel model);

		void ValidateModel(IIdentifiedModel model, ValidationResult validation);
	}
}
