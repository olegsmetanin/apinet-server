using System;

namespace AGO.Hibernate.Attributes.Mapping
{
	/// <summary>
	/// Предоставляет авто-маперу информацию о что референс-свойство является частью ассоциации, и как оно должно мапиться.
	/// По умолчанию референс-свойства мапятся и без этого атрибута, если не помечены NotMapped
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ManyToOneAttribute : Attribute
	{
		private CascadeType _CascadeType = CascadeType.None;
		public CascadeType CascadeType { get { return _CascadeType; } set { _CascadeType = value; } }
	}
}