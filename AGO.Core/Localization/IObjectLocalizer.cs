using System.Globalization;

namespace AGO.Core.Localization
{
	public interface IObjectLocalizer : ILocalizer
	{
		bool Accept(object obj);

		string Message(object obj, CultureInfo culture);
	}
}