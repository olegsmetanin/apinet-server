using System;
using AGO.Core.Model.Projects;

namespace AGO.Core.Model.Security
{
	public interface ISecureModel : IIdentifiedModel
	{
		ProjectMemberModel Creator { get; set; }

		DateTime? LastChangeTime { get; set; }

		ProjectMemberModel LastChanger { get; set; }
	}
}
