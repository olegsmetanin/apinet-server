using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	[TestFixture, Ignore]
	public class DbCreator: AbstractControllersApplication
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
