using AGO.Core.Application;

namespace AGO.Core.Application
{
	public interface ITestDataApplication : IApplication
	{
		void CreateDatabase();

		void PopulateDatabase();

		void CreateAndPopulateDatabase();
	}
}
