using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	public class AuthController : AbstractController
	{
		#region Constants

		public const string LoginName = "email";

		public const string PwdName = "password";
		
		#endregion

		#region Properties, fields, constructors

		public AuthController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public void Login(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);

			var loginProperty = request.Body.Property(LoginName);
			var login = loginProperty != null ? loginProperty.TokenValue().TrimSafe() : null;
			if (login == null || login.IsNullOrEmpty())
				throw new EmptyLoginException();

			var pwdProperty = request.Body.Property(PwdName);
			var pwd = pwdProperty != null ? pwdProperty.TokenValue().TrimSafe() : null;
			if (pwd == null || pwd.IsNullOrEmpty())
				throw new EmptyPwdException();

			var user = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == login).Take(1).List().FirstOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes(pwd)));
			if (!string.Equals(user.PwdHash, pwdHash))
				throw new InvalidPwdException();

			HttpContext.Current.Session["CurrentUser"] = user;

			_JsonService.CreateSerializer().Serialize(output, new
			{
				user = UserToJsonUser(user)
			});
		}

		[JsonEndpoint]
		public void Logout(JsonReader input, JsonWriter output)
		{
			HttpContext.Current.Session["CurrentUser"] = null;
		}

		[JsonEndpoint]
		public void CurrentUser(JsonReader input, JsonWriter output)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
				throw new NotAuthorizedException();

			_JsonService.CreateSerializer().Serialize(output, new
			{
				user = UserToJsonUser(currentUser)
			});
		}

		#endregion

		#region Public methods

		public bool IsAuthenticated
		{
			get { return GetCurrentUser() != null; }
		}

		public bool IsAdmin
		{
			get
			{
				var currentUser = GetCurrentUser();
				return currentUser != null && currentUser.SystemRole == SystemRole.Administrator;	
			}
		}

		public UserModel GetCurrentUser()
		{
			return HttpContext.Current.Session["CurrentUser"] as UserModel;
		}
		
		#endregion

		#region Helper methods

		protected object UserToJsonUser(UserModel user)
		{
			return new
			{
				id = user.Id,
				firstName = user.Name,
				lastName = user.LastName,
				email = user.Login,
				admin = user.SystemRole == SystemRole.Administrator
			};
		}

		#endregion
	}
}