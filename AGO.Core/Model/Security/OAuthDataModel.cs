using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;

namespace AGO.Core.Model.Security
{
	/// <summary>
	/// Model for storing OAuth data (subclasses will add own data specific to providers)
	/// </summary>
	[MetadataExclude, TablePerSubclass("ModelType"), RelationalModel]
	public abstract class OAuthDataModel: IdentifiedModel<Guid>
	{
	}

	//TODO move to separate file
	/// <summary>
	/// Facebook-specific oauth data
	/// </summary>
	public class FacebookOAuthDataModel : OAuthDataModel
	{
		/// <summary>
		/// Url, where user will be redirect to after login
		/// </summary>
		[NotLonger(1024)]
		public virtual string RedirectUrl { get; set; }

	}
}