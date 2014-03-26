using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using AGO.Core.DataAccess;
using AGO.Core.Model.Security;
using Newtonsoft.Json.Linq;

namespace AGO.Core.Controllers.Security.OAuth
{
	public class FacebookProvider: AbstractOAuthProvider
	{
		public FacebookProvider(ISessionProviderRegistry providerRegistry)
			: base(providerRegistry)
		{
		}

		public override OAuthProvider Type { get { return OAuthProvider.Facebook; } }

		public override Task<string> PrepareForLogin(OAuthDataModel data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			var fbUrl = string.Concat(loginUrl, "?response_type=code&client_id=", appId, 
				"&state=", data.Id.ToString().ToLowerInvariant(),
				"&redirect_uri=", RedirectUrl);
			return Task.FromResult(fbUrl);
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
		public override async Task<UserModel> QueryUserId(OAuthDataModel data, NameValueCollection parameters)
		{
			try
			{
				var code = parameters["code"];
				var exchangeUrl = string.Concat(graphUrl, "/oauth/access_token?client_id=", appId,
					"&redirect_uri=", RedirectUrl, "&client_secret=", appSecret, "&code=", code);

				using (var http = new HttpClient())
				{
					var response = await http.GetStringAsync(exchangeUrl).ConfigureAwait(false);
					//response contains access_token=kdfdlj&expires=12345, if no error
					var accessToken = response.Split('&', '=')[1];

					var meUrl = "https://graph.facebook.com/me?access_token=" + accessToken;
					response = await http.GetStringAsync(meUrl).ConfigureAwait(false);

					var jobj = JObject.Parse(response);
					var userId = jobj.TokenValue("id");
					var user = FindUserById(userId);
					if (user == null)
					{
						var fname = jobj.TokenValue("first_name");
						var lname = jobj.TokenValue("last_name");
						user = RegisterUser(userId, fname, lname);
					}


					if (user.AvatarUrl.IsNullOrWhiteSpace())
					{
						UpdateAvatar(user, "https://graph.facebook.com/" + userId + "/picture?width=23&height=23");
					}

					return user;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Error when retrieving userId from facebook: {0}", ex.ToString());
				throw new OAuthLoginException(ex);
			}
		}

		public override bool IsCancel(NameValueCollection parameters)
		{
			var error = parameters["error"];
			var code = parameters["error_code"];
			var reason = parameters["error_reason"];
			return !error.IsNullOrWhiteSpace() && !code.IsNullOrWhiteSpace() && !reason.IsNullOrWhiteSpace()
			       && error == "access_denied" && code == "200" && reason == "user_denied";
		}

		#region Configuration

		private string loginUrl;
		private string graphUrl;
		private string appId;
		private string appSecret;

		private const string LoginUrlConfigKey = "LoginUrl";
		private const string GraphUrlConfigKey = "GraphUrl";
		private const string AppIdConfigKey = "AppId";
		private const string AppSecretConfigKey = "AppSecret";

		protected override string DoGetConfigProperty(string key)
		{
			if (LoginUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return loginUrl;
			}
			if (GraphUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return graphUrl;
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
			if (LoginUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				loginUrl = value;
			}
			else if (GraphUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				graphUrl = value;
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
