using System;

namespace AGO.WebApiApp
{
	public class Global : global::System.Web.HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{

		}

		protected void Session_Start(object sender, EventArgs e)
		{
			#if DEBUG

			/*HttpContext.Current.Session["CurrentUser"] = 
				DependencyResolver.Current.GetService<ISessionProvider>().CurrentSession
				.QueryOver<UserModel>()
				.Where(m => m.Login == "admin@apinet-test.com")
				.Take(1).List().FirstOrDefault();*/

			#endif
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}