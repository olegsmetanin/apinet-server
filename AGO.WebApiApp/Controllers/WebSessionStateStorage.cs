using System.Web;
using AGO.Core.Controllers;

namespace AGO.WebApiApp.Controllers
{
	public class WebSessionStateStorage : IStateStorage
	{
		public object this[string key]
		{
			get
			{
				var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
				return session == null ? null : session[key];
			}
			set
			{
				var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
				if (session != null)
					session[key] = value;
			}
		}

		public void Remove(string key)
		{
			var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
			if (session != null)
				session.Remove(key);
		}

		public void RemoveAll()
		{
			var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
			if (session != null)
				session.RemoveAll();
		}
	}
}