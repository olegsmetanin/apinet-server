using System;
using System.Globalization;

namespace AGO.Core.Localization
{
	public interface ITypeLocalizerByKey : ILocalizer
	{
		bool Accept(Type type, string key);

		string Message(Type type, string key, CultureInfo culture, object[] args);
	}
}