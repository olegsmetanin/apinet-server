using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Application;

namespace AGO.WebApiApp.Application
{
	public class TestDataApplication : AbstractTestFixture, ITestDataApplication
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

		protected override IEnumerable<Type> ModuleTestDataServices
		{
			get
			{
				return base.ModuleTestDataServices.Concat(new[]
				{
					typeof(Home.ModuleTestDataService),
					typeof(Tasks.ModuleTestDataService)
				});
			}
		}
	}
}
