using System;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Dictionary
{
	public interface IDictionaryItemModel : ISecureModel, IProjectBoundModel
	{
		Guid Id { get; set; }

		string Name { get; set; }
	}
}
