using Microsoft.AspNet.SignalR;
using Owin;

namespace AGO.Notifications
{
    public sealed class Startup
    {
		public static void StartupAsPublisher(string signalRDbConnectionString)
		{
			GlobalHost.DependencyResolver.UseSqlServer(signalRDbConnectionString);
		}

		public static void StartupAsNotificationHost(IAppBuilder app, string signalRDbConnectionString)
		{
			GlobalHost.DependencyResolver.UseSqlServer(signalRDbConnectionString);
			var uidprov = new CookieUserIdProvider();
			GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => uidprov);
			app.MapSignalR();
		}
    }
}
