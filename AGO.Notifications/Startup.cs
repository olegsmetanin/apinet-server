namespace AGO.Notifications
{
    public sealed class Startup
    {
		public static void StartupAsPublisher(string signalRDbConnectionString)
		{
			//TODO replace with subscribe to redis
			//GlobalHost.DependencyResolver.UseSqlServer(signalRDbConnectionString);
		}

		//TODO replace with subscribe/publish to redis
		//public static void StartupAsNotificationHost(IAppBuilder app, string signalRDbConnectionString)
		//{
			//TODO replace with subscribe to redis
			//GlobalHost.DependencyResolver.UseSqlServer(signalRDbConnectionString);
			//var uidprov = new CookieUserIdProvider();
			//GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => uidprov);
			//app.MapSignalR();
		//}
    }
}
