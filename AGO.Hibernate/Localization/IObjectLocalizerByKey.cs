using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface IObjectLocalizerByKey : ILocalizer
	{
		bool Accept(object obj, string key);

		string Message(object obj, string key, CultureInfo culture);
	}
}