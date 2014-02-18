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
	public class OAuthDataModel: IdentifiedModel<Guid>
	{
		/// <summary>
		/// Url, where user will be redirect to after login
		/// </summary>
		[NotLonger(1024)]
		public virtual string RedirectUrl { get; set; }
	}

	/// <summary>
	/// Twitter-specific oauth data
	/// </summary>
	public class TwitterOAuthDataModel : OAuthDataModel
	{
		/// <summary>
		/// Request or access token
		/// </summary>
		[NotLonger(1024)]
		public virtual string Token { get; set; }

		/// <summary>
		/// Request or access token secret for signing messages
		/// </summary>
		[NotLonger(1024)]
		public virtual string TokenSecret { get; set; }
	}
}