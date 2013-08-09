using System;
using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface ITypeLocalizer : ILocalizer
	{
		bool Accept(Type type);

		string Message(Type type, CultureInfo culture, object[] args);
	}
}