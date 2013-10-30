using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.WebApi.Tests
{
	[TestFixture]
	public class DbBootstrap: AbstractTestFixture, ITestDataApplication
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

		[Test]
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