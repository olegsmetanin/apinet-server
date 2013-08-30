using System;

namespace AGO.Core.Attributes.Model
{
	/// <summary>
	/// Исключает модель или свойство из маппинга на реляционное хранилище
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public class NotMappedAttribute: Attribute
	{
	}
}