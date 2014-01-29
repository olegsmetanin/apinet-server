using System;
using AGO.Core.Application;

namespace AGO.WatchersHost
{
	public class Program
	{
		private static WatcherApplication app;

		public static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Starting watching application...");

				app = new WatcherApplication();
				app.Initialize();

				Console.WriteLine("Watching application started. For stop press Enter");
				Console.ReadLine();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
