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

				Console.WriteLine("Reporting service is running. Servicing projects '{0}'. Press Enter for stop", 
					service.ServicedProjects);
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
