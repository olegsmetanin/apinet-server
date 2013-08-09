using System;

namespace AGO.Hibernate.Filters
{
	public interface IFilterNode : ICloneable
	{
		string Path { get; }

		bool Negative { get; }
	}
}