using System;

namespace AGO.Reporting.Service
{
	public class Starter
	{
		private static ReportingService service;

		public static void Main(string[] args)
		{
			try
			{
				service = new ReportingService();
				service.Initialize();

				Console.WriteLine("Reporting service {0} is running. Servicing projects '{1}'. Press Enter for stop", 
					service.ServiceName, service.ServicedProjects);
				Console.ReadLine();

				service.Dispose();
				Console.WriteLine("Reporting service stopped");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
