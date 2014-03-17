using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AGO.Core.Model.Security;
using Newtonsoft.Json.Linq;
using Kvp = System.Tuple<string, string>;


namespace AGO.Core.Controllers.Security.OAuth
{
	public class TwitterProvider: AbstractOAuthProvider
	{
		public TwitterProvider(ISessionProvider sessionProvider):base(sessionProvider)
		{
		}

		public override OAuthProvider Type 
		{ 
			get { return OAuthProvider.Twitter; }
		}

		public override OAuthDataModel CreateData()
		{
			return new TwitterOAuthDataModel();
		}
		
		private string MakeAuthHeader(string method, string relativeUrl, string callback = null, string token = null, string secret = null, string verifier = null, string userId = null)
		{
			//kvp for each possible parameter, participating in header or signing
			var oauthNonce = new Kvp("oauth_nonce", Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
			var oauthCallbackUrl = callback != null ? new Kvp("oauth_callback", callback) : null;
			var oauthConsumerKey = new Kvp("oauth_consumer_key", consumerKey);
			var oauthTimestamp = new Kvp("oauth_timestamp", UnixTimestampUTC().ToString(CultureInfo.InvariantCulture));
			var oauthSignatureMethod = new Kvp("oauth_signature_method", "HMAC-SHA1");
			var oauthVersion = new Kvp("oauth_version", "1.0");
			var oauthToken = token != null ? new Kvp("oauth_token", token) : null;
			var oauthVerifier = verifier != null ? new Kvp("oauth_verifier", verifier) : null;
			var paramUserId = userId != null ? new Kvp("user_id", userId) : null;

			//sign-only parameters (kvp not needed)
			var requestUrl = apiUrl + relativeUrl;
			var signKey = Uri.EscapeDataString(consumerSecret) + "&" + Uri.EscapeDataString(secret ?? string.Empty);

			//signing
			var parametersForSigning = string.Join("&", new []
				{
					oauthNonce, oauthCallbackUrl, oauthConsumerKey, oauthTimestamp,
					oauthSignatureMethod, oauthVersion, oauthToken, oauthVerifier,
					paramUserId
				}
				.Where(kvp => kvp != null)
				.OrderBy(kvp => kvp.Item1)
				.Select(kvp => Uri.EscapeDataString(kvp.Item1) + "=" + Uri.EscapeDataString(kvp.Item2)));
			var signString = string.Concat(method, "&", Uri.EscapeDataString(requestUrl), "&",
				Uri.EscapeDataString(parametersForSigning)); //parameters double percent encoded
			var signature = Sign(signKey, signString);

			var oauthSignature = new Kvp("oauth_signature", signature);

			var headerValue = "OAuth " + string.Join(", ", new[]
				{
					oauthNonce, oauthCallbackUrl, oauthConsumerKey, oauthTimestamp,
					oauthSignatureMethod, oauthSignature, oauthVersion, oauthToken
				}
				.Where(kvp => kvp != null)
				.Select(kvp => Uri.EscapeDataString(kvp.Item1) + "=\"" + Uri.EscapeDataString(kvp.Item2) + "\""));
			return headerValue;
		}

		public override async Task<string> PrepareForLogin(OAuthDataModel data)
		{
			var twiData = (TwitterOAuthDataModel) data;
			const string requestTokenUrl = "oauth/request_token";
			var callbackUrl = RedirectUrl + "?state=" + data.Id.ToString().ToLowerInvariant();
			var authHeaderValue = MakeAuthHeader("POST", requestTokenUrl, callbackUrl);

			using (var http = new HttpClient())
			{
				var body = string.Empty;
				try
				{
					http.BaseAddress = new Uri(apiUrl);
					
					var request = new HttpRequestMessage(HttpMethod.Post, requestTokenUrl);
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
					DoInSession(s => s.SaveOrUpdate(twiData));
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

		public override async Task<UserModel> QueryUserId(OAuthDataModel data, NameValueCollection parameters)
		{
			var body = string.Empty;
			try
			{
				var twiData = (TwitterOAuthDataModel) data;
				var oauth_token_value = parameters["oauth_token"];
				var oauth_verifier_value = parameters["oauth_verifier"];

				if (!twiData.Token.Equals(oauth_token_value, StringComparison.InvariantCultureIgnoreCase))
					throw new InvalidOperationException(
						string.Format("Request token and recived token does not match. Request token '{0}', recieved token '{1}'", 
						twiData.Token, oauth_token_value));

				const string accessTokenUrl = "oauth/access_token";
				var authHeaderValue = MakeAuthHeader("POST", accessTokenUrl, token: oauth_token_value, verifier: oauth_verifier_value);

				using (var http = new HttpClient())
				{
					http.BaseAddress = new Uri(apiUrl);
					var request = new HttpRequestMessage(HttpMethod.Post, accessTokenUrl);
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
					//var screenName = parsedBody[7]; may be need later

					const string showUrl = "1.1/users/show.json";
					request = new HttpRequestMessage(HttpMethod.Get, showUrl + "?user_id=" + userId);
					authHeaderValue = MakeAuthHeader("GET", showUrl,  token: token, secret: secret, userId: userId);
					request.Headers.TryAddWithoutValidation("Authorization", authHeaderValue);
					response = await http.SendAsync(request).ConfigureAwait(false);
					body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					response.EnsureSuccessStatusCode();

					var jobj = JObject.Parse(body);
					var user = FindUserById(userId);
					if (user == null)
					{
						var name = jobj.TokenValue("name");
						var parts = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
						var fname = parts[0];
						var lname = parts.Length > 1 ? parts[1] : null;
						user = RegisterUser(userId, fname, lname);
					}
					
					var profileImageUrl = jobj.TokenValue("profile_image_url_https");
					UpdateAvatar(user, profileImageUrl);

					return user;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Error when get userId from twitter:\r\nBody:{0}\r\nException:{1}", body, ex.ToString());
				throw new OAuthLoginException(ex);
			}
		}

		public override bool IsCancel(NameValueCollection parameters)
		{
			return !parameters["denied"].IsNullOrWhiteSpace();
		}

		private static string Sign(string key, string signString)
		{
			var keyBytes = Encoding.ASCII.GetBytes(key);
			using (var hmac = new HMACSHA1(keyBytes))
			{
				var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signString));
				return Convert.ToBase64String(hash);
			}
		}

		private static int UnixTimestampUTC()
		{
			var utcNow = DateTime.UtcNow;
			var unixEpoch = new DateTime(1970, 1, 1);
			var unixTimestamp = (int)(utcNow.Subtract(unixEpoch)).TotalSeconds;
			return unixTimestamp;
		}

		#region Configuration

		private string apiUrl;
		private string consumerKey;
		private string consumerSecret;

		private const string ApiUrlConfigKey = "ApiUrl";
		private const string ConsumerKeyConfigKey = "ConsumerKey";
		private const string ConsumerSecretConfigKey = "ConsumerSecret";

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
			else
				base.DoSetConfigProperty(key, value);
		}

		#endregion
	}
}
