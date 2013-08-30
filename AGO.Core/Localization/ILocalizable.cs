using System.Collections.Generic;

namespace AGO.Core.Localization
{
	public interface ILocalizable
	{
		IEnumerable<object> MessageArguments { get; }
	}
}
