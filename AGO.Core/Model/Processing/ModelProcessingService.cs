using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Model.Projects;
using NHibernate;
using AGO.Core.Localization;

namespace AGO.Core.Model.Processing
{
	public class ModelProcessingService : AbstractService, IModelProcessingService
	{
		#region Properties, fields, constructors

		private readonly ILocalizationService localizationService;

		private readonly ISet<IModelValidator> modelValidators;

		private readonly ISet<IModelPostProcessor> modelPostProcessors;

		public ModelProcessingService(
			ILocalizationService localizationService,
			IEnumerable<IModelValidator> modelValidators,
			IEnumerable<IModelPostProcessor> modelPostProcessors)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			this.localizationService = localizationService;

			if (modelValidators == null)
				throw new ArgumentNullException("modelValidators");
			this.modelValidators = new HashSet<IModelValidator>(modelValidators);

			if (modelPostProcessors == null)
				throw new ArgumentNullException("modelPostProcessors");
			this.modelPostProcessors = new HashSet<IModelPostProcessor>(modelPostProcessors);
		}

		#endregion

		#region Interfaces implementation

		public void ValidateModelSaving(IIdentifiedModel model, ValidationResult validation, ISession session, object capability = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			foreach (var validator in modelValidators.OrderBy(m => m.Priority).Where(v => v.Accepts(model)))
				validator.ValidateModelSaving(model, validation, session, capability);
		}

		public void ValidateModelDeletion(IIdentifiedModel model, ValidationResult validation, ISession session, object capability = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			foreach (var validator in modelValidators.OrderBy(m => m.Priority).Where(v => v.Accepts(model)))
				validator.ValidateModelDeletion(model, validation, session, capability);
		}

		public void RegisterModelValidators(IEnumerable<IModelValidator> validators)
		{
			modelValidators.UnionWith(validators ?? Enumerable.Empty<IModelValidator>());
		}

		public void AfterModelCreated(IIdentifiedModel model, ProjectMemberModel creator = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");

			foreach (var postProcessor in modelPostProcessors.Where(v => v.Accepts(model)))
				postProcessor.AfterModelCreated(model, creator);
		}

		public void AfterModelUpdated(IIdentifiedModel model, IIdentifiedModel original, ProjectMemberModel changer = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");
			if (original == null)
				throw new ArgumentNullException("original");

			foreach (var postProcessor in modelPostProcessors.Where(v => v.Accepts(model)))
				postProcessor.AfterModelUpdated(model, original, changer);
		}

		public void AfterModelDeleted(IIdentifiedModel model, ProjectMemberModel deleter = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");

			foreach (var postProcessor in modelPostProcessors.Where(v => v.Accepts(model)))
				postProcessor.AfterModelDeleted(model, deleter);
		}

		public void RegisterModelPostProcessors(IEnumerable<IModelPostProcessor> postProcessors)
		{
			modelPostProcessors.UnionWith(postProcessors ?? Enumerable.Empty<IModelPostProcessor>());
		}

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			localizationService.TryInitialize();

			foreach (var modelValidator in modelValidators)
				modelValidator.TryInitialize();
		}

		#endregion
	}
}