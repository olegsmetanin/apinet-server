using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using AGO.Core;
using AGO.Reporting.Common;
using AGO.Reporting.Service;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class ReportingServiceTest: AbstractReportingTest
	{
		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();
		}

		[TestFixtureTearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		private IReportingService svc;

		[SetUp]
		public void SetUp()
		{
			svc = new ReportingService();
			svc.TryInitialize();
		}

		[TearDown]
		public new void TearDown()
		{
			svc.Dispose();
			svc = null;
			base.TearDown();
		}

		[Test]
		public void PingAlwaysReturnTrue()
		{
			Assert.IsTrue(svc.Ping());
		}

		[Test]
		public void ServiceDoMainStepsForCustomGenerator()
		{
			var tpl = M.Template("fake", Encoding.UTF8.GetBytes("<range_data>\"{$num$}\",\"{$txt$}\"</range_data>"));
			var stg = M.Setting("fake", tpl.Id, GeneratorType.CvsGenerator, typeof (FakeControlledGenerator).AssemblyQualifiedName);
			var task = M.Task("controlled", "Fast reports", stg.Id, "{a:1, b:'zxc'}");
			_SessionProvider.FlushCurrentSession();

			//svc.RunReport();

		}
	}

	public class FakeControlledGenerator: ICustomReportGenerator
	{
		public int Counter;
		public readonly AutoResetEvent Latch = new AutoResetEvent(false);

//		public XmlDocument GetReportData(string reportParams)
//		{
//			for (int i = 0; i < Counter; i++)
//			{
//				Latch.WaitOne();
//			}
//			return new XmlDocument();
//		}

		public Stream Result
		{
			get
			{
				var data = Encoding.UTF8.GetBytes("aaa");
				var ms = new MemoryStream(); 
				ms.Write(data, 0, data.Length);
				return ms;
			}
		}

		public string FileName { get; set; }

		public void MakeReport(string parameters, Func<Guid, string> templateResolver, Guid mainTemplateId)
		{
			for (int i = 0; i < Counter; i++)
			{
				Latch.WaitOne();
			}
		}
	}
}