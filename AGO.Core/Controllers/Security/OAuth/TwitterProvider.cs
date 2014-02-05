using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AGO.Core.Model.Security;


namespace AGO.Core.Controllers.Security.OAuth
{
	public class TwitterProvider: AbstractService, IOAuthProvider
	{
		private readonly ISessionProvider sp;

		public TwitterProvider(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");

			sp = sessionProvider;
		}

		public OAuthProvider Type { get { return OAuthProvider.Twitter; }
		}
		public OAuthDataModel CreateData()
		{
			return new TwitterOAuthDataModel();
		}

		public async Task<string> PrepareForLogin(OAuthDataModel data, string sourceUrl)
		{
			var twiData = (TwitterOAuthDataModel) data;
			var nonce = Uri.EscapeDataString(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
			var oauthCallbackUrl = Uri.EscapeDataString(redirectUrl + "?state=" + data.Id.ToString().ToLowerInvariant());
			var requestTokenUrl = apiUrl + "oauth/request_token";
			var timestamp = UnixTimeStampUTC().ToString(CultureInfo.InvariantCulture);

			//in signing order (alphabet)
// ReSharper disable InconsistentNaming
			var oauth_callback = "oauth_callback=\"" + oauthCallbackUrl + "\"";
			var oauth_consumer_key = "oauth_consumer_key=\"" + consumerKey + "\"";
			var oauth_nonce = "oauth_nonce=\"" + nonce + "\"";
			const string oauth_signature_method = "oauth_signature_method=\"HMAC-SHA1\"";
			var oauth_timestamp = "oauth_timestamp=\"" + timestamp + "\"";
			const string oauth_version = "oauth_version=\"1.0\"";

			var parametersString = string.Concat(
				"oauth_callback=", oauthCallbackUrl,
				"&oauth_consumer_key=", consumerKey,
				"&oauth_nonce=", nonce,
				"&oauth_signature_method=HMAC-SHA1", 
				"&oauth_timestamp=", timestamp,
				"&oauth_version=1.0");
			var signString = string.Concat("POST", "&", Uri.EscapeDataString(requestTokenUrl), "&",
				Uri.EscapeDataString(parametersString));
			var signKey = Uri.EscapeDataString(consumerSecret) + "&" /*no token_secret yet*/;
			var sign = Sign(signKey, signString);
			var oauth_signature = "oauth_signature=\"" + Uri.EscapeDataString(sign) + "\"";
// ReSharper restore InconsistentNaming
			

			var authHeaderValue = string.Concat("OAuth ", string.Join(", ", 
				oauth_callback, oauth_consumer_key, oauth_nonce, oauth_signature_method, oauth_timestamp, oauth_signature, oauth_version));

			using (var http = new HttpClient())
			{
				string body = string.Empty;
				try
				{
					http.BaseAddress = new Uri(apiUrl);
					var request = new HttpRequestMessage(HttpMethod.Post, "oauth/request_token");
					request.Headers.TryAddWithoutValidation("Authorization", authHeaderValue);

					var response = await http.SendAsync(request).ConfigureAwait(false);
					body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					response.EnsureSuccessStatusCode();

					if (string.IsNullOrWhiteSpace(body))
						throw new InvalidOperationException("Empty response from twitter api");
					var parsedBody = body.Split('=', '&');
					if (parsedBody.Length < 6)
						throw new InvalidOperationException(string.Format("Response from twitter api contains less parts than required: {0}", body));
// ReSharper disable InconsistentNaming
					var oauth_token = parsedBody[1];
					var oauth_token_secret = parsedBody[3];
					var oauth_callback_confirmed = bool.Parse(parsedBody[5]);

					if (!oauth_callback_confirmed)
						throw new InvalidOperationException("Request token not confirmed");

					twiData.Token = oauth_token;
					twiData.TokenSecret = oauth_token_secret;
					using (var s = sp.SessionFactory.OpenSession())
					{
						s.SaveOrUpdate(twiData);
						s.Flush();
						s.Close();
					}
// Rearper restore InconsistentNaming

					return apiUrl + "oauth/authenticate?oauth_token=" + oauth_token;
				}
				catch (Exception ex)
				{
					Log.ErrorFormat("Error when get request_token from twitter:\r\nBody:{0}\r\nException:{1}", body, ex.ToString());
					throw new OAuthLoginException(ex);
				}
			}
		}

		private string Sign(string key, string signString)
		{
			var keyBytes = Encoding.ASCII.GetBytes(key);
			using (var hmac = new HMACSHA1(keyBytes))
			{
				var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signString));
				return Convert.ToBase64String(hash);
			}
		}

		public async Task<string> QueryUserId(OAuthDataModel data, NameValueCollection parameters)
		{
			var body = string.Empty;
			try
			{
				var twiData = (TwitterOAuthDataModel) data;
// ReSharper disable InconsistentNaming
				var oauth_token_value = parameters["oauth_token"];
				var oauth_verifier_value = parameters["oauth_verifier"];

				if (!twiData.Token.Equals(oauth_token_value, StringComparison.InvariantCultureIgnoreCase))
					throw new InvalidOperationException(
						string.Format("Request token and recived token does not match. Request token '{0}', recieved token '{1}'", 
						twiData.Token, oauth_token_value));

				//Exchange request token to access token and get user id in one request
				var nonce = Uri.EscapeDataString(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
				var accessTokenUrl = apiUrl + "oauth/access_token";
				var timestamp = UnixTimeStampUTC().ToString(CultureInfo.InvariantCulture);

				//in signing order (alphabet)
				var oauth_consumer_key = "oauth_consumer_key=\"" + consumerKey + "\"";
				var oauth_nonce = "oauth_nonce=\"" + nonce + "\"";
				const string oauth_signature_method = "oauth_signature_method=\"HMAC-SHA1\"";
				var oauth_timestamp = "oauth_timestamp=\"" + timestamp + "\"";
				var oauth_token = "oauth_token=\"" + oauth_token_value + "\"";
				const string oauth_version = "oauth_version=\"1.0\"";

				var parametersString = string.Concat(
					"&oauth_consumer_key=", consumerKey,
					"&oauth_nonce=", nonce,
					"&oauth_signature_method=HMAC-SHA1",
					"&oauth_timestamp=", timestamp,
					"&oauth_token=", oauth_token_value,
					"&oauth_verifier=", oauth_verifier_value,
					"&oauth_version=1.0");
				var signString = string.Concat("POST", "&", Uri.EscapeDataString(accessTokenUrl), "&",
					Uri.EscapeDataString(parametersString));
				var signKey = Uri.EscapeDataString(consumerSecret) + "&" /*no token_secret yet*/;
				var sign = Sign(signKey, signString);
				var oauth_signature = "oauth_signature=\"" + Uri.EscapeDataString(sign) + "\"";

				var authHeaderValue = string.Concat("OAuth ", string.Join(", ",
					 oauth_consumer_key, oauth_nonce, oauth_signature_method, oauth_signature, oauth_timestamp, oauth_token, oauth_version));

				using (var http = new HttpClient())
				{
					http.BaseAddress = new Uri(apiUrl);
					var request = new HttpRequestMessage(HttpMethod.Post, "oauth/access_token");
					request.Headers.TryAddWithoutValidation("Authorization", authHeaderValue);
					var content = new StringContent("oauth_verifier=" + oauth_verifier_value, Encoding.UTF8);
					content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
					request.Content = content;

					var response = await http.SendAsync(request).ConfigureAwait(false);
					body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					response.EnsureSuccessStatusCode();

					if (string.IsNullOrWhiteSpace(body))
						throw new InvalidOperationException("Empty response from twitter api");

					var parsedBody = body.Split('&', '=');
					if (parsedBody.Length < 8)
						throw new InvalidOperationException(string.Format("Response from twitter api contains less parts than required: {0}", body));
					var token = parsedBody[1];
					var secret = parsedBody[3];
					var userId = parsedBody[5];
					var screenName = parsedBody[7];

					return userId;
				}

// ReSharper enable InconsistentNaming
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Error when get userId from twitter:\r\nBody:{0}\r\nException:{1}", body, ex.ToString());
				throw new OAuthLoginException(ex);
			}
		}

		private static int UnixTimeStampUTC()
		{
			var utcNow = DateTime.UtcNow;
			var unixEpoch = new DateTime(1970, 1, 1);
			var unixTimeStamp = (int)(utcNow.Subtract(unixEpoch)).TotalSeconds;
			return unixTimeStamp;
		}

		#region Configuration

		private string apiUrl;
		private string consumerKey;
		private string consumerSecret;
		private string redirectUrl;

		private const string ApiUrlConfigKey = "ApiUrl";
		private const string ConsumerKeyConfigKey = "ConsumerKey";
		private const string ConsumerSecretConfigKey = "ConsumerSecret";
		private const string RedirectUrlConfigKey = "RedirectUrl";

		protected override string DoGetConfigProperty(string key)
		{
			if (ApiUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return apiUrl;
			}
			if (ConsumerKeyConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return consumerKey;
			}
			if (ConsumerSecretConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return consumerSecret;
			}
			if (RedirectUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return redirectUrl;
			}
			return base.DoGetConfigProperty(key);
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			if (ApiUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				apiUrl = value;
			}
			else if (ConsumerKeyConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				consumerKey = value;
			}
			else if (ConsumerSecretConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				consumerSecret = value;
			}
			else if (RedirectUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				redirectUrl = value;
			}
			else
				base.DoSetConfigProperty(key, value);
		}

		#endregion
	}
}