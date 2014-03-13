using System;
using System.Data;
using System.Linq;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Localization;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;


namespace AGO.Core.Controllers.Security
{
	public sealed class AuthController : AbstractService
	{
		#region Properties, fields, constructors

		private readonly ISessionProvider sessionProvider;

		private readonly IStateStorage<object> stateStorage;

		private readonly ILocalizationService localizationService;

		public AuthController(
			ISessionProvider sessionProvider,
			IStateStorage<object> stateStorage,
			ILocalizationService localizationService)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			this.sessionProvider = sessionProvider;

			if (stateStorage == null)
				throw new ArgumentNullException("stateStorage");
			this.stateStorage = stateStorage;

			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			this.localizationService = localizationService;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint]
		public object LoginAsDemo()
		{
			var validation = new ValidationResult();

			var user = sessionProvider.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Email == "demo@apinet-test.com").Take(1).List().FirstOrDefault();
			if (user == null)
			{
				validation.AddFieldErrors("email", localizationService.MessageForException(new NoSuchUserException()));
				return validation;
			}
			
			LoginInternal(user);
			
			return CurrentUserDto();
		}

		public void LoginInternal(UserModel user)
		{
			//TODO избавиться от сессии (stateless)
			stateStorage["CurrentUser"] = user;
			stateStorage["CurrentUserToken"] = RegisterToken(user.Email).ToString();
		}

		[JsonEndpoint]
		public bool Logout()
		{
			//TODO избавиться от сессии (stateless)
			stateStorage.Remove("CurrentUser");
			stateStorage.Remove("CurrentUserToken");
			return true;
		}

		[JsonEndpoint]
		public bool IsAuthenticated()
		{
			return CurrentUser() != null;
		}

		public UserModel CurrentUser()
		{
			//TODO избавиться от сессии (stateless)
			return  stateStorage["CurrentUser"] as UserModel;
		}

		[JsonEndpoint, RequireAuthorization]
		public object CurrentUserDto()
		{
			//TODO избавиться от сессии (stateless)
			var user = stateStorage["CurrentUser"] as UserModel;
			return user == null ? null : UserToJsonUser(user, stateStorage["CurrentUserToken"] as string);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool IsAdmin()
		{
			return CurrentUser().SystemRole == SystemRole.Administrator;
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
			var conn = sessionProvider.CurrentSession.Connection;
			var cmd = conn.CreateCommand();
			cmd.CommandText = RegisterTokenCmd;

			var pLogin = cmd.CreateParameter();
			pLogin.ParameterName = "login";
			pLogin.DbType = DbType.String;
			pLogin.Size = UserModel.EMAIL_SIZE;
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

		#region Helper methods

		private object UserToJsonUser(UserModel user, string token)
		{
			return new
			{
				user.Id,
				Login = user.Email,
				user.FirstName,
				user.LastName,
				user.FullName,
				user.SystemRole,
				user.AvatarUrl,
				Token = token
			};
		}

		#endregion
	}
}