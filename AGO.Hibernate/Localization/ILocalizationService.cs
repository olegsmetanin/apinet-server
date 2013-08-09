using System;
using System.Collections.Generic;
using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface ILocalizationService
	{
		IEnumerable<CultureInfo> Cultures { get; }

		string Message(object key, CultureInfo culture = null, params object[] args);

		string MessageFor<TType>(object key = null, CultureInfo culture = null, params object[] args)
			where TType : class;

		string MessageForType(Type type, object key = null, CultureInfo culture = null, params object[] args);

		string MessageFor(object obj, object key = null, CultureInfo culture = null);	

		string MessageForException(Exception exception, CultureInfo culture = null);
	}
}