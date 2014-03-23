using System;

namespace AGO.Core.Model.Dictionary
{
	public interface IDictionaryItemModel : IProjectBoundModel
	{
		Guid Id { get; set; }

		string Name { get; set; }
	}
}
