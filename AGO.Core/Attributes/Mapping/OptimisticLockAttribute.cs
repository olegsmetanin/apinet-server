using System;

namespace AGO.Core.Attributes.Mapping
{
	/// <summary>
	/// Помечает что модель использует Optimistic locking
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class OptimisticLockAttribute : Attribute
	{
		private OptimisticLockType _LockType = OptimisticLockType.All;
		public OptimisticLockType LockType { get { return _LockType; } set { _LockType = value; } }
	}
}
