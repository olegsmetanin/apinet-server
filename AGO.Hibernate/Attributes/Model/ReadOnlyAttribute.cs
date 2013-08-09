﻿using System;

namespace AGO.Hibernate.Attributes.Model
{
	/// <summary>
	/// Помечает свойства которые присутствуют в хранилище, но не могут быть сохранены в него средствами DAL 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ReadOnlyAttribute: Attribute
	{
	}
}