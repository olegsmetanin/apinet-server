using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Model;

namespace AGO.Core.Validation
{
	public class ValidationService : AbstractService, IValidationService
	{
		#region Properties, fields, constructors

		protected readonly IList<IModelValidator> _Validators;

		public ValidationService(IEnumerable<IModelValidator> validators)
		{
			if (validators == null)
				throw new ArgumentNullException("validators");
			_Validators = new List<IModelValidator>(validators);
		}

		#endregion

		#region Interfaces implementation

		public void RegisterModelValidator(IModelValidator validator)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (validator == null)
				throw new ArgumentNullException("validator");
			_Validators.Add(validator);
		}

		public void ValidateModel(IIdentifiedModel model, ValidationResult validation)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			foreach (var validator in _Validators.OrderBy(m => m.Priority).Where(v => v.Accepts(model)))
				validator.ValidateModel(model, validation);
		}

		#endregion
	}
}