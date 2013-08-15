using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Docstore.Model.Documents;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Dictionary.Documents
{
	public class DocumentCategoryModel : SecureModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Предшественник"), /*JsonProperty,*/]
		public virtual DocumentCategoryModel Parent { get; set; }

		[DisplayName("Последователи"), PersistentCollection]
		public virtual ISet<DocumentCategoryModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<DocumentCategoryModel> _Children = new HashSet<DocumentCategoryModel>();

		[DisplayName("Документы"), PersistentCollection]
		public virtual ISet<DocumentModel> Documents { get { return _Documents; } set { _Documents = value; } }
		private ISet<DocumentModel> _Documents = new HashSet<DocumentModel>();	

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
