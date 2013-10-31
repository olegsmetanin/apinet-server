using System.Collections.Generic;

namespace AGO.Core.Controllers
{
	public interface IStateStorage<TType>
	{
		IEnumerable<string> Keys { get; }

		TType this[string key] { get; set; }

		void Remove(string key);

		void RemoveAll();
	}
}
