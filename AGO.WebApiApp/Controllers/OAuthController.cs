using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers.Security;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Model.Security;

namespace AGO.WebApiApp.Controllers
{
	public class OAuthController: Controller
	{
		private IDependencyResolver Resover
		{
			get { return DependencyResolver.Current; }
		}

		public async Task<ActionResult> BeginFacebookLoginFlow()
		{
			var sourceUrl = Request.QueryString["url"];
			//TODO check args
			var provider = Resover.GetService<IOAuthProviderFactory>().Get(OAuthProvider.Facebook);
			var data = provider.CreateData();
			data.RedirectUrl = sourceUrl;
			Resover.GetService<ICrudDao>().Store(data);
			Resover.GetService<ISessionProvider>().FlushCurrentSession();

			var redirectUrl = await provider.PrepareForLogin(data, sourceUrl);
			return Redirect(redirectUrl);
		}

		//Not use async, because our StateStorage implementation relies on HttpContext.Current, that is null
		//in async method
		public ActionResult EndFacebookLoginFlow()
		{
			var state = Request.QueryString["state"];

			var provider = Resover.GetService<IOAuthProviderFactory>().Get(OAuthProvider.Facebook);
			var data = Resover.GetService<ICrudDao>().Get<OAuthDataModel>(new Guid(state));
			var oauthUserId = provider.QueryUserId(data, Request.QueryString).Result;

			var user = Resover.GetService<ISessionProvider>().CurrentSession.QueryOver<UserModel>()
				.Where(m => m.OAuthProvider == OAuthProvider.Facebook && m.OAuthUserId == oauthUserId).SingleOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			Resover.GetService<AuthController>().LoginInternal(user);

			return Redirect(data.RedirectUrl);
		}

		public async Task<ActionResult> BeginTwitterLoginFlow()
		{
			var sourceUrl = Request.QueryString["url"];
			//TODO check args
			var provider = Resover.GetService<IOAuthProviderFactory>().Get(OAuthProvider.Twitter);
			var data = provider.CreateData();
			data.RedirectUrl = sourceUrl;
			Resover.GetService<ICrudDao>().Store(data);
			Resover.GetService<ISessionProvider>().FlushCurrentSession();

			var redirectUrl = await provider.PrepareForLogin(data, sourceUrl);
			return Redirect(redirectUrl);
		}

		//See comment for facebook version
		public ActionResult EndTwitterLoginFlow()
		{
			var state = Request.QueryString["state"];

			var provider = Resover.GetService<IOAuthProviderFactory>().Get(OAuthProvider.Twitter);
			var data = Resover.GetService<ICrudDao>().Get<OAuthDataModel>(new Guid(state));
			var oauthUserId = provider.QueryUserId(data, Request.QueryString).Result;

			var user = Resover.GetService<ISessionProvider>().CurrentSession.QueryOver<UserModel>()
				.Where(m => m.OAuthProvider == OAuthProvider.Twitter && m.OAuthUserId == oauthUserId).SingleOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			Resover.GetService<AuthController>().LoginInternal(user);

			return Redirect(data.RedirectUrl);
		}
	}
}
