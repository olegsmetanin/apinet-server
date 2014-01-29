using NUnit.Framework;

namespace AGO.WorkQueue.Tests
{
	[TestFixture]
	public class PostgreSqlQueueConsumptionTest: AbstractQueueConsumptionTest
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			PostgreSqlHelper.CreateDbAndSchema();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			PostgreSqlHelper.DropDb();
		}

		public override void TearDown()
		{
			PostgreSqlHelper.ExecuteBatch(PostgreSqlHelper.ConnStr, "truncate work_queue");
			base.TearDown();
		}

		protected override IWorkQueue CreateQueue()
		{
			return new PostgreSqlWorkQueue(PostgreSqlHelper.ConnStr);
		}
	}
}