﻿using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Localization;

namespace AGO.Core.Model.Processing
{
	public class AttributeValidatingModelValidator : IModelValidator
	{
		#region Properties, fields, constructors

		protected readonly ILocalizationService localizationService;

		public AttributeValidatingModelValidator(ILocalizationService localizationService)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			this.localizationService = localizationService;
		}

		#endregion

		#region Interfaces implementation

		public int Priority { get { return int.MinValue; } }

		public bool Accepts(IIdentifiedModel model)
		{
			return true;
		}

		public void ValidateModelSaving(
			IIdentifiedModel model, 
			ValidationResult validation,
			ISession session,
			object capability = null)
		{
			if (model == null)
				throw new ArgumentNullException("model");
			if (validation == null)
				throw new ArgumentNullException("validation");

			DoValidateModelSaving(model, validation, session, capability, null);
		}

		public void ValidateModelDeletion(
			IIdentifiedModel model,
			ValidationResult validation,
			ISession session,
			object capability = null)
		{
		}

		#endregion

		#region Template methods

		protected virtual void DoValidateModelSaving(
			object model,
			ValidationResult validation,
			ISession session,
			object capability,
			string namePrefix)
		{
			var properties = model.GetType().GetProperties().Where(
				pi => pi.FirstAttribute<NotMappedAttribute>(true) == null && pi.CanRead && pi.CanWrite).ToArray();

			foreach (var propertyInfo in properties)
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
						DoValidateModelSaving(component, validation, session, capabilityObj, propertyInfo.Name);
						continue;
					}

				var invalidAttributes = propertyInfo.FindInvalidPropertyConstraintAttributes(value);
				if (capabilityValue ?? false)
				{
					invalidAttributes.UnionWith(Extensions.FindInvalidItemConstraintAttributes(
						propertyInfo.PropertyType, value, null, new NotEmptyAttribute(),
						Enumerable.Empty<InRangeAttribute>(), Enumerable.Empty<RegexValidatedAttribute>()));
				}

					var uniquePropertyAttribute = propertyInfo.FirstAttribute<UniquePropertyAttribute>(true);
					if (uniquePropertyAttribute != null)
					{
						var criteria = session.CreateCriteria(model.GetType())
							.Add(value != null 
								? Restrictions.Eq(propertyInfo.Name, value)
								: Restrictions.IsNull(propertyInfo.Name))
						    .Add(Restrictions.Not(Restrictions.IdEq(model.GetMemberValue("Id"))));

						foreach (var groupProperty in uniquePropertyAttribute.GroupProperties
							.Select(model.GetType().GetProperty).Where(p => p.CanRead))
						{
							var groupValue = groupProperty.GetValue(model, null);
							criteria = criteria.Add(groupValue != null 
								? Restrictions.Eq(groupProperty.Name, groupValue) 
								: Restrictions.IsNull(groupProperty.Name));
						}

						if (criteria.SetProjection(Projections.RowCount()).UniqueResult<int>() > 0)
						invalidAttributes.UnionWith(new[] { uniquePropertyAttribute });
					}
						
				foreach (var invalidAttribute in invalidAttributes)
				{
					try
					{
					if (invalidAttribute is NotNullAttribute || invalidAttribute is NotEmptyAttribute)
					{
						if (capabilityValue ?? true)
						throw new RequiredValueException();
						continue;
					}

						if (invalidAttribute is UniquePropertyAttribute)
							throw new MustBeUniqueException();

						if (invalidAttribute is RegexValidatedAttribute)
							throw new MustMatchRegexException();

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
					var msg = localizationService.MessageForException(e);
					validation.AddFieldErrors(string.Format("{0}{1}", namePrefix, propertyInfo.Name), msg);
				}
			}
		}
		}

		#endregion
	}
}
