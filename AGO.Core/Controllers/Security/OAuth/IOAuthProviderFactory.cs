namespace AGO.Core.Controllers.Security.OAuth
{
	/// <summary>
	/// Interface for retrieving providers via DI
	/// </summary>
	public interface IOAuthProviderFactory
	{
		IOAuthProvider Get(OAuthProvider type);
	}
}