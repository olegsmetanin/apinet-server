using System;
using System.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Localization;
using AGO.Core.Model;
using NHibernate.Criterion;

namespace AGO.Core.Validation
{
	public class AttributeValidatingModelValidator : IModelValidator
	{
		protected readonly ILocalizationService _LocalizationService;

		protected readonly ISessionProvider _SessionProvider;

		public AttributeValidatingModelValidator(
			ILocalizationService localizationService,
			ISessionProvider sessionProvider)
		{
			if(localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		public int Priority { get { return int.MinValue; } }

		public bool Accepts(IIdentifiedModel model)
		{
			return true;
		}

		public void ValidateModel(IIdentifiedModel model, ValidationResult validation)
		{
			var properties = model.GetType().GetProperties().Where(
				pi => pi.FirstAttribute<NotMappedAttribute>(true) == null && pi.CanRead && pi.CanWrite).ToArray();

			foreach (var propertyInfo in properties)
			{
				try
				{
					var value = propertyInfo.GetValue(model, null);

					var uniquePropertyAttribute = propertyInfo.FirstAttribute<UniquePropertyAttribute>(true);
					if (uniquePropertyAttribute != null && _SessionProvider.CurrentSession.CreateCriteria(model.GetType())
							.Add(Restrictions.Eq(propertyInfo.Name, value))
							.Add(Restrictions.Not(Restrictions.IdEq(model.GetMemberValue("Id"))))
							.SetProjection(Projections.RowCount()).UniqueResult<int>() > 0)
						throw new MustBeUniqueException();

					var invalidAttribute = propertyInfo.FindInvalidPropertyConstraintAttribute(value);
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
					validation.AddFieldErrors(propertyInfo.Name, msg);
				}
			}
		}
	}
}
