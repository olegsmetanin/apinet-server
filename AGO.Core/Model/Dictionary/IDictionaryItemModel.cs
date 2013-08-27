using System;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Dictionary
{
	public interface IDictionaryItemModel : ISecureModel
	{
		Guid Id { get; set; }

		string ProjectCode { get; set; }

		string Name { get; set; }
	}
}
