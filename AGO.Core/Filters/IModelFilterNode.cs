using System.Collections.Generic;

namespace AGO.Core.Filters
{
	public interface IModelFilterNode : IFilterNode
	{
		ModelFilterOperators? Operator { get; }

		IEnumerable<IFilterNode> Items { get; }

		void AddItem(IFilterNode node);
	}
}
