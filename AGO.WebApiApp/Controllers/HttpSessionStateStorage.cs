using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AGO.Core;
using AGO.Core.Controllers;

namespace AGO.WebApiApp.Controllers
{
	public class HttpSessionStateStorage : IStateStorage<object>
	{
		#region Interfaces implementation

		public IEnumerable<string> Keys
		{
			get
			{
				var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
				return session == null ? Enumerable.Empty<string>() : session.Keys.OfType<string>();
			}
		}

		public object this[string key]
		{
			get
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
				return session == null ? null : session[key];
			}
			set
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
				if (session != null)
					session[key] = value;
			}
		}

		public void Remove(string key)
		{
			if (key.IsNullOrWhiteSpace())
				throw new ArgumentNullException("key");

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

		#endregion
	}
}