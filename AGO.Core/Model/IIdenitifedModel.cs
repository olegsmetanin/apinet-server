using System;

namespace AGO.Core.Model
{
	public interface IIdentifiedModel : IComparable, IComparable<IIdentifiedModel>, ICloneable
	{
		string UniqueId { get; }

		DateTime? CreationTime { get; }

		string Description { get; set; }

		Type RealType { get; }

		bool IsNew();
	}

	public interface IIdentifiedModel<out TId> : IIdentifiedModel
	{
		TId Id { get; }
	}
}
