using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	public class CustomPropertyTypeModel : SecureModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Формат"), JsonProperty, NotLonger(64)]
		public virtual string Format { get; set; }

		[DisplayName("Тип значения"), JsonProperty, EnumDisplayNames(new[]
		{
			"String", "Строка",
			"Number", "Число",
			"Date", "Дата"
		})]
		public virtual CustomPropertyValueType ValueType { get; set; }

		[DisplayName("Предшественник"), JsonProperty]
		public virtual CustomPropertyTypeModel Parent { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ParentId { get; set; }

		[DisplayName("Последователи"), PersistentCollection]
		public virtual ISet<CustomPropertyTypeModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<CustomPropertyTypeModel> _Children = new HashSet<CustomPropertyTypeModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
