using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AGO.Core.Model.Security;
using FluentNHibernate.Utils;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Controllers.Security.OAuth
{
	public class FacebookProvider: AbstractService, IOAuthProvider
	{
		private readonly ISessionProvider sp;

		public FacebookProvider(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");

			sp = sessionProvider;
		}

		public OAuthProvider Type { get { return OAuthProvider.Facebook; } }

		public OAuthDataModel CreateData()
		{
			return new FacebookOAuthDataModel();
		}

		public Task<string> PrepareForLogin(OAuthDataModel data, string sourceUrl)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			if (!(data is FacebookOAuthDataModel))
				throw new ArgumentException("Invalid type of provider oauth data", "data");
			if (string.IsNullOrWhiteSpace(sourceUrl))
				throw new ArgumentNullException("sourceUrl");

			var fbData = (FacebookOAuthDataModel) data;
			fbData.RedirectUrl = sourceUrl;
			sp.CurrentSession.SaveOrUpdate(fbData);
			sp.FlushCurrentSession();

			return Task.FromResult(url + "?response_type=code&client_id=" + appId + "&state=fb:" + fbData.Id.ToString().ToLowerInvariant());
		}

		/// <summary>
		/// Why ConfigureAwait(false)?
		/// http://msdn.microsoft.com/en-us/magazine/gg598924.aspx
		/// http://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
		/// http://stackoverflow.com/a/9343733/3222246
		/// <para>
		/// The reason you may need AsyncContext.RunTask instead of Task.WaitAndUnwrapException is because of a rather 
		/// subtle deadlock possibility that happens on WinForms/WPF/SL/ASP.NET:
		/// A synchronous method calls an async method, obtaining a Task.
		/// The synchronous method does a blocking wait on the Task.
		/// The async method uses await without ConfigureAwait.
		/// The Task cannot complete in this situation because it only completes when the async method is finished; 
		/// the async method cannot complete because it is attempting to schedule its continuation to the SynchronizationContext, 
		/// and WinForms/WPF/SL/ASP.NET will not allow the continuation to run because the synchronous method is already running in that context.
		/// </para>
		/// 
		/// And this article provide great explanation for deadlock scenario too
		/// http://msdn.microsoft.com/en-us/magazine/jj991977.aspx Figure 3
		/// </summary>
		public async Task<string> QueryUserId(OAuthDataModel data, string code)
		{
			var fbData = (FacebookOAuthDataModel) data;
			var exchangeUrl = "https://graph.facebook.com/oauth/access_token?client_id=" + appId +
			                  "&redirect_uri=" + Uri.EscapeDataString(fbData.RedirectUrl) +
			                  "&client_secret=" + appSecret +
			                  "&code=" + code;
			try
			{
				using (var http = new HttpClient())
				{
					var response = await http.GetStringAsync(exchangeUrl).ConfigureAwait(false);
					//response contains access_token=kdfdlj&expires=12345, if no error
					var accessToken = response.Split('&', '=')[1];

					var meUrl = "https://graph.facebook.com/me?access_token=" + accessToken;
					response = await http.GetStringAsync(meUrl).ConfigureAwait(false);

					var jobj = JObject.Parse(response);
					var userId = jobj.TokenValue("id");
					
					return userId;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Error when retrieving userId from facebook: {0}", ex);
				throw new OAuthLoginException(ex);
			}
		}

		#region Configuration

		private string url;
		private string appId;
		private string appSecret;

		private const string UrlConfigKey = "Url";
		private const string AppIdConfigKey = "AppId";
		private const string AppSecretConfigKey = "AppSecret";

		protected override string DoGetConfigProperty(string key)
		{
			if (UrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return url;
			}
			if (AppIdConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return appId;
			}
			if (AppSecretConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return appSecret;
			}
			return base.DoGetConfigProperty(key);
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			if (UrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				url = value;
			} 
			else if (AppIdConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				appId = value;
			} 
			else if (AppSecretConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				appSecret = value;
			}
			else
				base.DoSetConfigProperty(key, value);
		}

		#endregion
	}
}