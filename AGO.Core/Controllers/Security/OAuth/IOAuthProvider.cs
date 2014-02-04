using System.Threading.Tasks;
using AGO.Core.Model.Security;

namespace AGO.Core.Controllers.Security.OAuth
{
	/// <summary>
	/// Abstraction for OAuth providers, used for authentication in apinet system.
	/// Some or all of provider methods require http-interaction with provider server, 
	/// so, all method follow a async/await scheme
	/// </summary>
	/// <remarks>First implementation will be contains providers for Facebook and Twitter</remarks>
	public interface IOAuthProvider
	{
		/// <summary>
		/// Provider type
		/// </summary>
		OAuthProvider Type { get; }

		/// <summary>
		/// Create data for storing needed info between requests (factory method)
		/// </summary>
		/// <returns>Empty instance of OAuthDataModel</returns>
		OAuthDataModel CreateData();

		/// <summary>
		/// Prepares for login. Do provider-specific preparation steps and return url for client redirecting
		/// to provider login form
		/// </summary>
		/// <returns>Url to provider login form</returns>
		Task<string> PrepareForLogin(OAuthDataModel data, string sourceUrl);

		/// <summary>
		/// Query provider and return provider user identifier
		/// </summary>
		/// <returns>Provider-specific user identifier</returns>
		Task<string> QueryUserId(OAuthDataModel data, string code);
	}
}