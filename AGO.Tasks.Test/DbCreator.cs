using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	[TestFixture]
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
