using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.System.Controllers
{
	internal class AuthInfo
	{
		public string Login { get; set; }

		public IList<string> Roles { get; set; }
	}

	public class AuthController : AbstractController
	{
		#region Constants

		public const string LoginName = "email";

		public const string PwdName = "password";

		public const string AdminRoleName = "admin";
		
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

			UserModel alias = null;
			var authInfo = new AuthInfo
			{
				Login = user.Login,
				Roles = _SessionProvider.CurrentSession.QueryOver<RoleModel>()
					.JoinAlias(m => m.Users, () => alias)
					.Where(() => alias.Id == user.Id)
					.Select(m => m.Name)
					.List<string>()
			};
			HttpContext.Current.Session["AuthInfo"] = authInfo;

			_JsonService.CreateSerializer().Serialize(output, new
			{
				user = new
				{
					id = user.Id,
					firstName = user.Name,
					lastName = user.LastName,
					email = user.Login,
					admin = authInfo.Roles.Contains(AdminRoleName)
				}
			});
		}

		[JsonEndpoint]
		public void Logout(JsonReader input, JsonWriter output)
		{
			HttpContext.Current.Session["AuthInfo"] = null;
		}

		[JsonEndpoint]
		public void CurrentUser(JsonReader input, JsonWriter output)
		{
			var authInfo = HttpContext.Current.Session["AuthInfo"] as AuthInfo;
			if (authInfo == null)
				throw new NotAuthorizedException();

			var user = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == authInfo.Login).Take(1).List().FirstOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			_JsonService.CreateSerializer().Serialize(output, new
			{
				user = new
				{
					id = user.Id,
					firstName = user.Name,
					lastName = user.LastName,
					email = user.Login,
					admin = authInfo.Roles.Contains(AdminRoleName)
				}
			});
		}

		#endregion

		#region Public methods

		public bool IsAuthenticated()
		{
			return (HttpContext.Current.Session["AuthInfo"] as AuthInfo) != null;
		}

		public bool HasAnyRole(params string[] roles)
		{
			var authInfo = HttpContext.Current.Session["AuthInfo"] as AuthInfo;
			return authInfo != null && (roles ?? new string[0]).Any(role => authInfo.Roles.Contains(role));
		}

		public bool HasAllRoles(params string[] roles)
		{
			var authInfo = HttpContext.Current.Session["AuthInfo"] as AuthInfo;
			return authInfo != null && (roles ?? new string[0]).All(role => authInfo.Roles.Contains(role));
		}

		#endregion
	}
}