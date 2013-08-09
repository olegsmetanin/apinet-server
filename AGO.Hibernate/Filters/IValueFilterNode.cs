namespace AGO.Hibernate.Filters
{
	public interface IValueFilterNode : IFilterNode
	{
		ValueFilterOperators? Operator { get; }

		string Operand { get; }

		bool IsUnary { get; }

		bool IsBinary { get; }
	}
}