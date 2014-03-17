using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers.Security;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Model.Security;

namespace AGO.WebApiApp.Controllers
{
	public class OAuthController: BaseMvcController
	{
		private IDependencyResolver Resolver
		{
			get { return DependencyResolver.Current; }
		}

		public Task<ActionResult> BeginLoginFlow(OAuthProvider providerType)
		{
			return DoSafeAsync(async () =>
			{
				var sourceUrl = Request.QueryString["url"];
				if (sourceUrl.IsNullOrWhiteSpace())
					throw new ArgumentException("No return url in query string");
				
				var provider = Resolver.GetService<IOAuthProviderFactory>().Get(providerType);
				var data = provider.CreateData();
				data.RedirectUrl = sourceUrl;
				Resolver.GetService<ICrudDao>().Store(data);
				Resolver.GetService<ISessionProvider>().FlushCurrentSession();

				var redirectUrl = await provider.PrepareForLogin(data);
				return Redirect(redirectUrl);
			});
		}

		//Not use async, because our StateStorage implementation relies on HttpContext.Current, that is null
		//in async method
		public ActionResult EndLoginFlow(OAuthProvider providerType)
		{
			return DoSafe(() =>
			{
				OAuthDataModel data = null;
				try
				{
					var state = Request.QueryString["state"];
					if (state.IsNullOrWhiteSpace())
						throw new ArgumentException("No state parameter in query string");
					Guid dataId;
					if (!Guid.TryParse(state, out dataId))
						throw new ArgumentException("Provided state parameter is not convertible to guid");

					var provider = Resolver.GetService<IOAuthProviderFactory>().Get(providerType);
					data = Resolver.GetService<ICrudDao>().Get<OAuthDataModel>(dataId);

					if (provider.IsCancel(Request.QueryString))
					{
						return Redirect(data.RedirectUrl);
					}

					var user = provider.QueryUserId(data, Request.QueryString).Result;
					Resolver.GetService<AuthController>().LoginInternal(user);

					return Redirect(data.RedirectUrl);
				}
				finally
				{
					if (data != null)
					{
						Resolver.GetService<ICrudDao>().Delete(data);
						Resolver.GetService<ISessionProvider>().CloseCurrentSession();
					}
				}
			});
		}
	}
}
