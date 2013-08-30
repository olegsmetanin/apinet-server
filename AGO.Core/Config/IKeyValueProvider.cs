using System.Collections.Generic;

namespace AGO.Core.Config
{
	public interface IKeyValueProvider
	{
		IEnumerable<string> Keys { get; }

		string RealKey(string key);

		string Value(string key);
	}
}