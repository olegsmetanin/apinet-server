using System.Collections.Generic;

namespace AGO.Hibernate.Filters
{
	public interface IModelFilterNode : IFilterNode
	{
		ModelFilterOperators? Operator { get; }

		IEnumerable<IFilterNode> Items { get; }

		void AddItem(IFilterNode node);
	}
}
