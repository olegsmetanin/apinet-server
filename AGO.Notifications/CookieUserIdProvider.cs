using Microsoft.AspNet.SignalR;

namespace AGO.Notifications
{
	public sealed class CookieUserIdProvider: IUserIdProvider
	{
		public string GetUserId(IRequest request)
		{
			return request.QueryString["login"];

			//return request.Cookies["login"] != null ? request.Cookies["login"].Value : null;
		}
	}
}
