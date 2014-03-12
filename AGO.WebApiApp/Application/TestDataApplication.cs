using System;
using AGO.Core.Application;
using AGO.Core.Notification;

namespace AGO.WebApiApp.Application
{
	public class TestDataApplication : AbstractControllersApplication, ITestDataApplication
	{
		public void CreateDatabase()
		{
			DoCreateDatabase();
		}

		public void PopulateDatabase()
		{
			Initialize();
			DoPopulateDatabase();
		}

		public void CreateAndPopulateDatabase()
		{
			DoCreateDatabase();

			Initialize();
			DoPopulateDatabase();
		}
		protected override Type NotificationServiceType { get { return typeof(NoopNotificationService); } }
	}
}
