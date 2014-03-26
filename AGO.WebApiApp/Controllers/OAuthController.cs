using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AGO.Core;
using AGO.Core.Controllers.Security;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.DataAccess;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.WebApiApp.Controllers
{
	public class OAuthController: BaseMvcController
	{
		private IDependencyResolver Resolver
		{
			get { return DependencyResolver.Current; }
		}

		private T DoWithSession<T>(Func<ISession, T> action)
		{
			var registry = Resolver.GetService<ISessionProviderRegistry>();
			var mainDb = registry.GetMainDbProvider();
			try
			{
				return action(mainDb.CurrentSession);
			}
			finally
			{
				registry.CloseCurrentSessions();
			}
		}

		public Task<ActionResult> BeginLoginFlow(OAuthProvider providerType)
		{
			return DoSafeAsync(() => DoWithSession<Task<ActionResult>>(async session =>
			{
				var sourceUrl = Request.QueryString["url"];
				if (sourceUrl.IsNullOrWhiteSpace())
					throw new ArgumentException("No return url in query string");

				var provider = Resolver.GetService<IOAuthProviderFactory>().Get(providerType);
				var data = provider.CreateData();
				data.RedirectUrl = sourceUrl;
				session.Save(data);
				session.Flush();

				var redirectUrl = await provider.PrepareForLogin(data);
				return Redirect(redirectUrl);
			}));
		}

		//Not use async, because our StateStorage implementation relies on HttpContext.Current, that is null
		//in async method
		public ActionResult EndLoginFlow(OAuthProvider providerType)
		{
			return DoSafe(() => DoWithSession<ActionResult>(session =>
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
					data = session.Get<OAuthDataModel>(dataId);

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
						session.Delete(data);
						session.Flush();
					}
				}
			}));
		}
	}
}
