using System;
using System.Globalization;

namespace AGO.Core.Localization
{
	public interface IExceptionLocalizer : ILocalizer
	{
		bool Accept(Exception exception);

		string Message(Exception exception, CultureInfo culture);
	}
}