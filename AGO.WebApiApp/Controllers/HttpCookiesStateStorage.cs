using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AGO.Core;
using AGO.Core.Controllers;

namespace AGO.WebApiApp.Controllers
{
	public class HttpCookiesStateStorage : IStateStorage<string>
	{
		#region Interfaces implementation

		public IEnumerable<string> Keys
		{
			get
			{
				var cookies = HttpContext.Current != null ? HttpContext.Current.Request.Cookies : null;
				var result = cookies == null ? Enumerable.Empty<string>() : cookies.AllKeys;

				cookies = HttpContext.Current != null ? HttpContext.Current.Response.Cookies : null;
				return result.Union(cookies == null ? Enumerable.Empty<string>() : cookies.AllKeys);
			}
		}

		public string this[string key]
		{
			get
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				var cookies = HttpContext.Current != null ? HttpContext.Current.Request.Cookies : null;
				var cookie = cookies != null ? cookies.Get(key) : null;

				return cookie == null ? null : cookie.Value;
			}
			set
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				var cookies = HttpContext.Current != null ? HttpContext.Current.Response.Cookies : null;
				if (cookies == null)
					return;

				cookies.Remove(key);
				cookies.Add(new HttpCookie(key, value) { Expires = DateTime.UtcNow.AddMonths(1) });
			}
		}

		public void Remove(string key)
		{
			if (key.IsNullOrWhiteSpace())
				throw new ArgumentNullException("key");

			var cookies = HttpContext.Current != null ? HttpContext.Current.Request.Cookies : null;
			if (cookies != null)
				cookies.Remove(key);

			cookies = HttpContext.Current != null ? HttpContext.Current.Response.Cookies : null;
			if (cookies != null)
				cookies.Remove(key);
		}

		public void RemoveAll()
		{
			var cookies = HttpContext.Current != null ? HttpContext.Current.Request.Cookies : null;
			if (cookies != null)
				cookies.Clear();
			
			cookies = HttpContext.Current != null ? HttpContext.Current.Response.Cookies : null;
			if (cookies != null)
				cookies.Clear();
		}

		#endregion
	}
}