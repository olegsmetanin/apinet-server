using System.Collections.Generic;
using System.Globalization;

namespace AGO.Hibernate.Localization
{
	public interface ILocalizer
	{
		IEnumerable<CultureInfo> Cultures { get; }
	}
}
