using AGO.Core.Application;
using Npgsql;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	[TestFixture, Ignore]
	public class DbCreator: AbstractPersistenceApplication
	{
		[Test]
		public void Recreate()
		{
			DoCreateDatabase();

			Initialize();
			DoPopulateDatabase();
		}
	}
}
