using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface ILocalizerByKey : ILocalizer
	{
		bool Accept(string key);

		string Message(string key, CultureInfo culture, object[] args);
	}
}