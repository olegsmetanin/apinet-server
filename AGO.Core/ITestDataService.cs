using System.Collections.Generic;

namespace AGO.Core
{
	public interface ITestDataService
	{
		IEnumerable<string> RequiredDatabases { get; }

		void Populate();
	}
}
