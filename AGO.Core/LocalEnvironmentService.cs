using System.IO;
using System.Net;

namespace AGO.Core
{
	public class LocalEnvironmentService : AbstractService, IEnvironmentService
	{
		#region Interfaces implementation

		public IPAddress EnvironmentIp { get { return IPAddress.Loopback; } }

		public IPAddress ClientIp { get { return IPAddress.Loopback; } }

		public string ApplicationPath { get { return ApplicationAssembliesPath; } }

		public string ApplicationAssembliesPath
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(
					GetType().Assembly.Location) ?? string.Empty, string.Empty);
			}
		}
		
		public bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path.TrimSafe());
		}

		public string EnvironmentPath(string virtualPath)
		{
			return virtualPath.TrimSafe();
		}

		public string PhysicalPath(string virtualPath)
		{
			return Path.GetFullPath(virtualPath.TrimSafe());
		}

		#endregion
	}
}
