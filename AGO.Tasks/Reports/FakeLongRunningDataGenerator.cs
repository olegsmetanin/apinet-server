using System.Globalization;
using System.Threading;
using AGO.Reporting.Common;

namespace AGO.Tasks.Reports
{
	public class FakeLongRunningDataGenerator : BaseReportDataGenerator
	{
		protected override void FillReportData(object parameters)
		{
			const int iter = 100;
			InitTicker(iter);
			var range = MakeRange("data", "fake");
			for (var i = 0; i < iter; i++)
			{
				var item = MakeItem(range);
				MakeValue(item, "num", i.ToString(CultureInfo.InvariantCulture));
				Thread.Sleep(1000);
				Ticker.AddTick();
			}
		}
	}
}