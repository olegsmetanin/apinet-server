using AGO.Core.Model.Reporting;
using AGO.Core.Tests;
using AGO.Reporting.Common;

namespace AGO.Reporting.Tests
{
	public class AbstractReportingTest : AbstractPersistenceTest<ModelHelper>
	{
		protected override void DoRegisterPersistence()
		{
			base.DoRegisterPersistence();
			IocContainer.Register<IReportingRepository, ReportingRepository>();
		}

		public override void TearDown()
		{
			ExecuteNonQuery(@"truncate ""Core"".""WorkQueue""", MainSession.Connection);
			base.TearDown();
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => Session, () => CurrentUser);
			M = new ModelHelper(() => Session, () => CurrentUser);
		}
	}
}
