using System;

namespace AGO.Core.Attributes.Mapping
{
	/// <summary>
	/// Предоставляет авто-маперу информацию о что коллекция является персистентной, и параметры как она должна мапиться
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PersistentCollectionAttribute: Attribute
	{
		public string Column { get; set; }

		public string LinkTable { get; set; }

		private bool _Inverse = true;
		public bool Inverse { get { return _Inverse; } set { _Inverse = value; } }

		private CascadeType _CascadeType = CascadeType.None;
		public CascadeType CascadeType { get { return _CascadeType; } set { _CascadeType = value; } }
	}
}