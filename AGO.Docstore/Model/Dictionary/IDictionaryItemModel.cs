using System;
using AGO.Docstore.Model.Security;

namespace AGO.Docstore.Model.Dictionary
{
	public interface IDictionaryItemModel : ISecureModel
	{
		Guid Id { get; set; }

		string ProjectCode { get; set; }

		string Name { get; set; }
	}
}
