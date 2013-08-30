using System.Collections.Generic;
using System.Globalization;

namespace AGO.Core.Localization
{
	public interface ILocalizer
	{
		IEnumerable<CultureInfo> Cultures { get; }
	}
}
