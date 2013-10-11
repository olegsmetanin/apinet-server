using System.Collections.Generic;

namespace AGO.Core.Model.Processing
{
	public interface IModelProcessingService
	{
		void ValidateModelSaving(IIdentifiedModel model, ValidationResult validation, object capability = null);

		void ValidateModelDeletion(IIdentifiedModel model, ValidationResult validation, object capability = null);

		void RegisterModelValidators(IEnumerable<IModelValidator> validators);

		bool CopyModelProperties(IIdentifiedModel target, IIdentifiedModel source, object capability = null);
	}
}
