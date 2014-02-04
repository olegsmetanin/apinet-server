using System;
using System.Collections.Generic;

namespace AGO.Core.Controllers.Security.OAuth
{
	/// <summary>
	/// Factory implementation (idea from https://simpleinjector.codeplex.com/wikipage?title=How-to#Resolve-Instances-By-Key)
	/// </summary>
	public class OAuthProviderFactory: Dictionary<OAuthProvider, Func<IOAuthProvider>>, IOAuthProviderFactory
	{
		public IOAuthProvider Get(OAuthProvider type)
		{
			return this[type]();
		}
	}
}