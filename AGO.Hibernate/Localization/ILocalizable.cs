using System.Collections.Generic;

namespace AGO.Hibernate.Localization
{
	public interface ILocalizable
	{
		IEnumerable<object> MessageArguments { get; }
	}
}
