using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface IObjectLocalizer : ILocalizer
	{
		bool Accept(object obj);

		string Message(object obj, CultureInfo culture);
	}
}