using System.Collections.Generic;

namespace AGO.Core.Model.Dictionary
{
	public interface IHierarchicalDictionaryItemModel<T> : IDictionaryItemModel where T: IHierarchicalDictionaryItemModel<T>, IIdentifiedModel
	{
		string FullName { get; set; }

		T Parent { get; }

		ISet<T> Children { get; }
	}
}
