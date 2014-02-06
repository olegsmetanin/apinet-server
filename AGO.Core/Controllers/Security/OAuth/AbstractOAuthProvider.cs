using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Controllers.Security.OAuth
{
	public abstract class AbstractOAuthProvider: AbstractService, IOAuthProvider
	{
		private readonly ISessionProvider sp;
		protected string RedirectUrl;
		private const string RedirectUrlConfigKey = "RedirectUrl";

		protected AbstractOAuthProvider(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");

			sp = sessionProvider;
		}

		protected void DoInSession(Action<ISession> action)
		{
			using (var s = sp.SessionFactory.OpenSession())
			{
				action(s);
				s.Flush();
				s.Close();
			}
		}

		protected virtual UserModel FindUserById(string userId)
		{
			if (userId.IsNullOrWhiteSpace())
				throw new ArgumentNullException("userId");

			UserModel u = null;
			DoInSession(s =>
			{
				u = s.QueryOver<UserModel>().Where(m => m.OAuthProvider == Type && m.OAuthUserId == userId).SingleOrDefault();
			});
			if (u == null)
				throw new NoSuchUserException();
			return u;
		}

		protected virtual void UpdateAvatar(UserModel user, string url)
		{
			DoInSession(s =>
			{
				user.AvatarUrl = url;
				s.Update(user);
			});
		}

		protected override string DoGetConfigProperty(string key)
		{
			if (RedirectUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				return RedirectUrl;
			}
			return base.DoGetConfigProperty(key);
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			if (RedirectUrlConfigKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				RedirectUrl = value;
			}
			else
				base.DoSetConfigProperty(key, value);
		}

		public virtual OAuthDataModel CreateData()
		{
			return new OAuthDataModel();
		}

		public abstract OAuthProvider Type { get; }
		public abstract Task<string> PrepareForLogin(OAuthDataModel data);
		public abstract Task<UserModel> QueryUserId(OAuthDataModel data, NameValueCollection parameters);
	}
}
