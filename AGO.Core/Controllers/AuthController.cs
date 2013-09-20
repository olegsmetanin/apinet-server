using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;

namespace AGO.Core.Controllers
{
	public class AuthController : AbstractService
	{
		#region Properties, fields, constructors

		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		protected readonly ICrudDao _CrudDao;

		protected readonly IFilteringDao _FilteringDao;

		protected readonly ISessionProvider _SessionProvider;

		public AuthController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider)
		{
			if (jsonService == null)
				throw new ArgumentNullException("jsonService");
			_JsonService = jsonService;

			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			_FilteringService = filteringService;

			if (crudDao == null)
				throw new ArgumentNullException("crudDao");
			_CrudDao = crudDao;

			if (filteringDao == null)
				throw new ArgumentNullException("filteringDao");
			_FilteringDao = filteringDao;

			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public UserModel Login([NotEmpty] string email, [NotEmpty] string password)
		{
			var user = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == email.TrimSafe()).Take(1).List().FirstOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes(password.TrimSafe())));
			if (!string.Equals(user.PwdHash, pwdHash))
				throw new InvalidPwdException();

			HttpContext.Current.Session["CurrentUser"] = user;

			return user;
		}

		[JsonEndpoint]
		public bool Logout()
		{
			HttpContext.Current.Session["CurrentUser"] = null;
			return true;
		}

		[JsonEndpoint]
		public bool IsAuthenticated()
		{
			return CurrentUser() != null;
		}

		[JsonEndpoint, RequireAuthorization]
		public UserModel CurrentUser()
		{
			return HttpContext.Current.Session["CurrentUser"] as UserModel;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool IsAdmin()
		{
			return CurrentUser().SystemRole == SystemRole.Administrator;
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