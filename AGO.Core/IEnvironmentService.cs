using System.Net;

namespace AGO.Core
{
	public interface IEnvironmentService
	{
		IPAddress EnvironmentIp { get; }

		IPAddress ClientIp { get; }

		string ApplicationPath { get; }

		string ApplicationAssembliesPath { get; }

		bool IsPathRooted(string path);

		string EnvironmentPath(string virtualPath);

		string PhysicalPath(string virtualPath);
	}
}