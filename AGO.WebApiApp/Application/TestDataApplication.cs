using AGO.Core.Application;

namespace AGO.WebApiApp.Application
{
	public class TestDataApplication : AbstractPersistenceApplication, ITestDataApplication
	{
		public void CreateDatabase()
		{
			DoCreateDatabase();
		}

		public void PopulateDatabase()
		{
			Initialize();
			DoPopulateDatabase();
		}

		public void CreateAndPopulateDatabase()
		{
			DoCreateDatabase();

			Initialize();
			DoPopulateDatabase();
		}
	}
}
