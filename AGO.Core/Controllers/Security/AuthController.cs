using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Security.OAuth;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;


namespace AGO.Core.Controllers.Security
{
	public class AuthController : AbstractService
	{
		#region Properties, fields, constructors

		protected readonly IJsonService _JsonService;

		protected readonly IFilteringService _FilteringService;

		protected readonly ICrudDao _CrudDao;

		protected readonly IFilteringDao _FilteringDao;

		protected readonly ISessionProvider _SessionProvider;

		protected readonly IStateStorage<object> _StateStorage;

		protected readonly ILocalizationService _LocalizationService;

		protected IOAuthProviderFactory OAuthFactory { get; private set; }

		public AuthController(
			IJsonService jsonService,
			IFilteringService filteringService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			IStateStorage<object> stateStorage,
			ILocalizationService localizationService,
			IOAuthProviderFactory oauthFactory)
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

			if (stateStorage == null)
				throw new ArgumentNullException("stateStorage");
			_StateStorage = stateStorage;

			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			_LocalizationService = localizationService;

			if (oauthFactory == null)
				throw new ArgumentNullException("oauthFactory");
			OAuthFactory = oauthFactory;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public object Login([NotEmpty] string email, [NotEmpty] string password)
		{
			var validation = new ValidationResult();

			var user = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == email.TrimSafe()).Take(1).List().FirstOrDefault();
			if (user == null)
			{
				validation.AddFieldErrors("email", _LocalizationService.MessageForException(new NoSuchUserException()));
				return validation;
			}

			var cryptoProvider = new MD5CryptoServiceProvider();
			var pwdHash = Encoding.Default.GetString(
				cryptoProvider.ComputeHash(Encoding.Default.GetBytes(password.TrimSafe())));
			if (!string.Equals(user.PwdHash, pwdHash))
			{
				validation.AddFieldErrors("password", _LocalizationService.MessageForException(new InvalidPwdException()));
				return validation;
			}
			
			LoginInternal(user);
			
			return user;
		}

		private void LoginInternal(UserModel user)
		{
			//TODO избавиться от сессии (stateless)
			user.Token = RegisterToken(user.Login);
			_StateStorage["CurrentUser"] = user;
		}

		[JsonEndpoint]
		public bool Logout()
		{
			_StateStorage.Remove("CurrentUser");
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
			return  _StateStorage["CurrentUser"] as UserModel;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool IsAdmin()
		{
			return CurrentUser().SystemRole == SystemRole.Administrator;
		}

		[JsonEndpoint]
		public string PrepareOAuthLogin([NotEmpty] OAuthProvider providerType, string sourceUrl)
		{
			var provider = OAuthFactory.Get(providerType);

			var data = provider.CreateData();
			_CrudDao.Store(data);
			_SessionProvider.FlushCurrentSession();

			return provider.PrepareForLogin(data, sourceUrl).Result;
		}

		[JsonEndpoint]
		public string ProceedOAuthLogin(OAuthProvider providerType, string code, Guid state)
		{
			var provider = OAuthFactory.Get(providerType);

			var data = _CrudDao.Get<OAuthDataModel>(state);
			var oauthUserId = provider.QueryUserId(data, code).Result;

			var user = _SessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.OAuthProvider == providerType && m.OAuthUserId == oauthUserId).SingleOrDefault();
			if (user == null)
				throw new NoSuchUserException();

			LoginInternal(user);

			return ((FacebookOAuthDataModel) data).RedirectUrl;
		}

		#endregion

		#region Token manipulation method

		private const string RegisterTokenCmd = @"
delete from ""Core"".""TokenToLogin"" where ""Login"" = :login and ""CreatedAt"" < :expireDate;
insert into ""Core"".""TokenToLogin"" (""Token"", ""Login"", ""CreatedAt"") values(:token, :login, :createDate);";

		private Guid RegisterToken(string login)
		{
			if (login.IsNullOrWhiteSpace())
				throw new ArgumentNullException("login");

			var token = Guid.NewGuid();
			var conn = _SessionProvider.CurrentSession.Connection;
			var cmd = conn.CreateCommand();
			cmd.CommandText = RegisterTokenCmd;

			var pLogin = cmd.CreateParameter();
			pLogin.ParameterName = "login";
			pLogin.DbType = DbType.String;
			pLogin.Size = UserModel.LOGIN_SIZE;
			pLogin.Value = login;
			var pExpireDate = cmd.CreateParameter();
			pExpireDate.ParameterName = "expireDate";
			pExpireDate.DbType = DbType.DateTime;
			pExpireDate.Value = DateTime.UtcNow.AddDays(-7);
			var pToken = cmd.CreateParameter();
			pToken.ParameterName = "token";
			pToken.DbType = DbType.Guid;
			pToken.Value = token;
			var pCreateDate = cmd.CreateParameter();
			pCreateDate.ParameterName = "createDate";
			pCreateDate.DbType = DbType.DateTime;
			pCreateDate.Value = DateTime.UtcNow;
			cmd.Parameters.Add(pLogin);
			cmd.Parameters.Add(pExpireDate);
			cmd.Parameters.Add(pToken);
			cmd.Parameters.Add(pCreateDate);

			cmd.ExecuteNonQuery();

			return token;
		}

		#endregion

		//Может понадобиться, когда из UserModel будем удалять Token и переделывать по нормальному
//		#region Helper methods
//
//		protected object UserToJsonUser(UserModel user, string token)
//		{
//			return new
//			{
//				id = user.Id,
//				firstName = user.Name,
//				lastName = user.LastName,
//				email = user.Login,
//				admin = user.SystemRole == SystemRole.Administrator,
//				token
//			};
//		}
//
//		#endregion
	}
}