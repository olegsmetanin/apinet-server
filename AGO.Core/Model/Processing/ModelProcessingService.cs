using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using AGO.Core.Localization;

namespace AGO.Core.Model.Processing
{
	public class ModelProcessingService : AbstractService, IModelProcessingService
	{
		#region Properties, fields, constructors

		protected readonly ILocalizationService _LocalizationService;

		protected readonly ICrudDao _CrudDao;

		protected ISessionProvider _SessionProvider;

		protected ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		protected readonly ISet<IModelValidator> _ModelValidators;

		public ModelProcessingService(
			ILocalizationService localizationService,
			ICrudDao crudDao,
			ISessionProvider sessionProvider,
			IEnumerable<IModelValidator> modelValidators)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;

			if (modelValidators == null)
				throw new ArgumentNullException("modelValidators");
			_ModelValidators = new HashSet<IModelValidator>(modelValidators);
		}

		#endregion

		#region Interfaces implementation

		public void ValidateModelSaving(IIdentifiedModel model, ValidationResult validation, object capability = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			foreach (var validator in _ModelValidators.OrderBy(m => m.Priority).Where(v => v.Accepts(model)))
				validator.ValidateModel(model, validation, capability);
		}

		public void RegisterModelValidators(IEnumerable<IModelValidator> validators)
		{
			_ModelValidators.UnionWith(validators ?? Enumerable.Empty<IModelValidator>());
		}

		public bool CopyModelProperties(IIdentifiedModel target, IIdentifiedModel source, object capability = null)
		{
			if (!_Ready)
				throw new ServiceNotInitializedException();

			if (target == null)
				throw new ArgumentNullException("target");

			return DoCopyModelProperties(target, source, capability);
		} 

		#endregion

		#region Template methods

		protected override void DoInitialize()
		{
			base.DoInitialize();

			_LocalizationService.TryInitialize();
			_SessionProvider.TryInitialize();
			_CrudDao.TryInitialize();

			foreach (var modelValidator in _ModelValidators)
				modelValidator.TryInitialize();
		}

		#endregion

		#region Helper methods

		protected void DoSetModelProperty(IIdentifiedModel model, PropertyInfo propertyInfo, IIdentifiedModel value)
		{
			propertyInfo.SetValue(model, value, null);

			if (value == null)
				return;
			
			var modelId = value.GetMemberValue("Id");
			if (modelId == null)
				return;

			var refIdProperty = model.GetType().GetProperty(propertyInfo.Name + "Id");
			if (refIdProperty == null || !refIdProperty.CanWrite || !refIdProperty.PropertyType.IsInstanceOfType(modelId))
				return;

			refIdProperty.SetValue(model, modelId, null);
		}

		protected bool DoCopyModelProperties(object target, object source, object capability)
		{
			if (target == null || source == null)
				return false;

			var targetType = target.GetType();
			var sourceType = source.GetType();
			var identifiedTarget = target as IIdentifiedModel;
			if (identifiedTarget != null)
				targetType = identifiedTarget.RealType;
			var identifiedSource = source as IIdentifiedModel;
			if (identifiedSource != null)
				sourceType = identifiedSource.RealType;

			if (!targetType.IsAssignableFrom(sourceType) &&
					!sourceType.IsAssignableFrom(targetType))
				return false;

			var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(pi => pi.CanRead && pi.CanWrite).ToArray();

			foreach (var propertyInfo in properties)
			{
				var otherProperty = source.GetType().GetProperty(propertyInfo.Name);
				if (otherProperty == null)
					continue;

				object capabilityObjectValue = null;
				if (capability != null)
				{
					var capabilityProperty = capability.GetType().GetProperty(propertyInfo.Name, typeof(bool?))
						?? capability.GetType().GetProperty(propertyInfo.Name);
					if (capabilityProperty == null)
						continue;
					capabilityObjectValue = capabilityProperty.GetValue(capability, null);
				}

				var isNullableValue = false;
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsNullable())
				{
					propertyType = propertyType.GetGenericArguments()[0];
					if (propertyType.IsEnum || propertyType.IsValueType)
						isNullableValue = true;
				}
				if (!propertyType.IsValueType &&
						!(typeof(string).IsAssignableFrom(propertyType) || typeof(IComponent).IsAssignableFrom(propertyType)))
					continue;

				var value = otherProperty.GetValue(source, null);
				if (value == null && !isNullableValue)
					continue;
				var existingValue = propertyInfo.GetValue(target, null);

				var component = value as IComponent;
				if (component != null)
				{
					DoCopyModelProperties(existingValue, component, capabilityObjectValue);
					continue;
				}

				var capabilityValue = capabilityObjectValue as bool?;
				if (capabilityValue != null && !capabilityValue.Value)
					continue;
				propertyInfo.SetValue(target, value, null);

				if (identifiedTarget == null)
					continue;

				var referenceProperty = propertyInfo.Name.EndsWith("Id")
					? target.GetType().GetProperty(propertyInfo.Name.RemoveSuffix("Id"))
					: null;
				if (referenceProperty != null && (!referenceProperty.CanWrite ||
						!typeof(IIdentifiedModel).IsAssignableFrom(referenceProperty.PropertyType)))
					referenceProperty = null;
				
				if (referenceProperty == null)
					continue;

				if (Equals(value, existingValue))
					continue;

				if (capability != null)
				{
					var capabilityProperty = capability.GetType().GetProperty(referenceProperty.Name, typeof(bool?));
					if (capabilityProperty != null)
					{
						capabilityValue = capabilityObjectValue as bool?;
						if (capabilityValue != null && !capabilityValue.Value)
							continue;
					}
				}

				var strVal = value as string;
				var referenceModel = value != null && (strVal == null || !strVal.IsNullOrWhiteSpace())
					? _CrudDao.Get<IIdentifiedModel>(value, false, referenceProperty.PropertyType)
					: null;

				DoSetModelProperty(identifiedTarget, referenceProperty, referenceModel);
			}

			return true;
		} 

		#endregion
	}
}