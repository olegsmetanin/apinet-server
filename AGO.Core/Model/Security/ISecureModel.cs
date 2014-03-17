using System;

namespace AGO.Core.Model.Security
{
	public interface ISecureModel<TUser> : IIdentifiedModel
	{
		TUser Creator { get; set; }

		DateTime? LastChangeTime { get; set; }

		TUser LastChanger { get; set; }
	}
}
