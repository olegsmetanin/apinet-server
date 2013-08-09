using System;

namespace AGO.Docstore.Model.Security
{
	public interface ISecureModel : IDocstoreModel
	{
		UserModel Creator { get; set; }

		DateTime? LastChangeTime { get; set; }

		UserModel LastChanger { get; set; }
	}
}
