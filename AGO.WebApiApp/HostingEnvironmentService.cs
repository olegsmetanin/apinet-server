using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;
using AGO.Core;

namespace AGO.WebApiApp
{
	public class HostingEnvironmentService : AbstractService, IEnvironmentService
	{
		#region Interfaces implementation

		public IPAddress EnvironmentIp { get { return IPAddress.Loopback; } }

		public IPAddress ClientIp
		{
			get
			{
				IPAddress result;
				return IPAddress.TryParse(HttpContext.Current.Request.UserHostAddress ?? string.Empty, out result)
					? result
					: IPAddress.Loopback;
			}
		}

		public string ApplicationPath { get { return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, string.Empty); } }

		public string ApplicationAssembliesPath { get { return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin"); } }

		public bool IsPathRooted(string path)
		{
			path = path.TrimSafe();
			return path.StartsWith("~\\") || path.StartsWith("~/") || Path.IsPathRooted(path);
		}

		public string EnvironmentPath(string virtualPath)
		{
			virtualPath = virtualPath.TrimSafe();
			
			if (virtualPath.StartsWith("\\") || virtualPath.StartsWith("/"))
				virtualPath = virtualPath.AddPrefix("~");

			return virtualPath;
		}

		public string PhysicalPath(string virtualPath)
		{
			return HostingEnvironment.MapPath(EnvironmentPath(virtualPath));
		}

		#endregion
	}
}
