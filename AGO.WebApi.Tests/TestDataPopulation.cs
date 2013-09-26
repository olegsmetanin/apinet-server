using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.WebApi.Tests
{
	[TestFixture]
	public class TestDataPopulation : AbstractTestFixture
	{
		[Test]
		public void CreateAndPopulateDatabase()
		{
			DoCreateDatabase();

			Initialize();
			DoPopulateDatabase();
		}

		[Test]
		public void PopulateDatabase()
		{
			Initialize();
			DoPopulateDatabase();
		}

		protected override IEnumerable<Type> TestDataPopulationServices
		{
			get
			{
				return base.TestDataPopulationServices.Concat(new[]
				{
					typeof(Home.TestDataPopulationService),
					typeof(Tasks.TestDataPopulationService)
				});
			}
		}
	}
}
