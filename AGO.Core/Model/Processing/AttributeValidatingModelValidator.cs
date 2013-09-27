﻿using System;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Localization;
using NHibernate.Criterion;

namespace AGO.Core.Model.Processing
{
	public class AttributeValidatingModelValidator : IModelValidator
	{
		#region Properties, fields, constructors

		protected readonly ILocalizationService _LocalizationService;

		protected readonly ISessionProvider _SessionProvider;

		public AttributeValidatingModelValidator(
			ILocalizationService localizationService,
			ISessionProvider sessionProvider)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		#endregion

		#region Interfaces implementation

		public int Priority { get { return int.MinValue; } }

		public bool Accepts(IIdentifiedModel model)
		{
			return true;
		}

		public void ValidateModel(
			IIdentifiedModel model, 
			ValidationResult validation,
			object capability = null)
		{
			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			DoValidateModelSaving(model, validation, capability, null);
		}

		#endregion

		#region Template methods

		protected virtual void DoValidateModelSaving(
			object model,
			ValidationResult validation,
			object capability,
			string namePrefix)
		{
			var properties = model.GetType().GetProperties().Where(
				pi => pi.FirstAttribute<NotMappedAttribute>(true) == null && pi.CanRead && pi.CanWrite).ToArray();

			foreach (var propertyInfo in properties)
			{
				var refIdProperty = typeof(IIdentifiedModel).IsAssignableFrom(propertyInfo.PropertyType)
					? model.GetType().GetProperty(propertyInfo.Name + "Id")
					: null;
				if (refIdProperty != null && !refIdProperty.CanWrite)
					refIdProperty = null;

				try
				{
					var capabilityProperty = capability != null 
						? capability.GetType().GetProperty(propertyInfo.Name)
						: null;
					var capabilityObj = capabilityProperty != null 
						? capabilityProperty.GetValue(capability, null) 
						: null;
					var capabilityValue = capabilityObj as bool?;

					var value = propertyInfo.GetValue(model, null);
					var component = value as IComponent;
					if (component != null)
					{
						DoValidateModelSaving(component, validation, capabilityObj, propertyInfo.Name);
						continue;
					}

					var uniquePropertyAttribute = propertyInfo.FirstAttribute<UniquePropertyAttribute>(true);
					if (uniquePropertyAttribute != null && _SessionProvider.CurrentSession.CreateCriteria(model.GetType())
							.Add(Restrictions.Eq(propertyInfo.Name, value))
							.Add(Restrictions.Not(Restrictions.IdEq(model.GetMemberValue("Id"))))
							.SetProjection(Projections.RowCount()).UniqueResult<int>() > 0)
						throw new MustBeUniqueException();

					var invalidAttribute = propertyInfo.FindInvalidPropertyConstraintAttribute(value);
					invalidAttribute = invalidAttribute ?? (capabilityValue != null && capabilityValue.Value
						? Extensions.FindInvalidItemConstraintAttribute(
							propertyInfo.PropertyType, value, null, new NotEmptyAttribute(), Enumerable.Empty<InRangeAttribute>())
						: null);

					if (invalidAttribute == null)
						continue;

					if (invalidAttribute is NotNullAttribute || invalidAttribute is NotEmptyAttribute)
						throw new RequiredValueException();

					var inRange = invalidAttribute as InRangeAttribute;
					if (inRange != null && inRange.Inclusive)
					{
						if (inRange.Start != null && inRange.End != null)
							throw new MustBeInRangeException(inRange.Start, inRange.End);
						if (inRange.Start != null)
							throw new MustBeGreaterOrEqualToException(inRange.Start);
						if (inRange.End != null)
							throw new MustBeLowerOrEqualToException(inRange.End);
					}
					if (inRange != null && !inRange.Inclusive)
					{
						if (inRange.Start != null && inRange.End != null)
							throw new MustBeBetweenException(inRange.Start, inRange.End);
						if (inRange.Start != null)
							throw new MustBeGreaterThanException(inRange.Start);
						if (inRange.End != null)
							throw new MustBeLowerThanException(inRange.End);
					}
					throw new InvalidOperationException();

				}
				catch (Exception e)
				{
					var msg = _LocalizationService.MessageForException(e);
					validation.AddFieldErrors(string.Format("{0}{1}", namePrefix, propertyInfo.Name), msg);
					if (refIdProperty != null)
						validation.AddFieldErrors(string.Format("{0}{1}", namePrefix, refIdProperty.Name), msg);
				}
			}
		}

		#endregion
	}
}