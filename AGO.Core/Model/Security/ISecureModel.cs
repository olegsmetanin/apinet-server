using System;

namespace AGO.Core.Model.Security
{
	public interface ISecureModel : IIdentifiedModel
	{
		UserModel Creator { get; set; }

		DateTime? LastChangeTime { get; set; }

		UserModel LastChanger { get; set; }
	}
}
