using System.Threading.Tasks;
using AGO.Core.Model.Security;

namespace AGO.Core.Controllers.Security.OAuth
{
	public class TwitterProvider: AbstractService, IOAuthProvider
	{
		public OAuthProvider Type { get { return OAuthProvider.Twitter; }
		}
		public OAuthDataModel CreateData()
		{
			throw new System.NotImplementedException();
		}

		public Task<string> PrepareForLogin(OAuthDataModel data, string sourceUrl)
		{
			throw new System.NotImplementedException();
		}

		public Task<string> QueryUserId(OAuthDataModel data, string code)
		{
			throw new System.NotImplementedException();
		}
	}
}