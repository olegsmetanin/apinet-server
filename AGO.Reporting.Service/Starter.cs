using System.Web.Hosting;


namespace AGO.Reporting.Service
{
	public class Starter: IProcessHostPreloadClient
	{
		private ReportingService service;

		public void Preload(string[] parameters)
		{
			service = new ReportingService();
			service.Initialize();
		}
	}
}
