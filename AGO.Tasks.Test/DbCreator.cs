using System;
using AGO.Core.Application;
using AGO.Core.Notification;
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

		protected override Type NotificationServiceType
		{
			get { return typeof (NoopNotificationService); }
		}
	}
}
