using System;

namespace AGO.Core.Filters
{
	public interface IFilterNode : ICloneable
	{
		string Path { get; }

		bool Negative { get; }
	}
}