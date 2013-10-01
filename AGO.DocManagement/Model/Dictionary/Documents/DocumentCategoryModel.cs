using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.DocManagement.Model.Documents;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.DocManagement.Model.Dictionary.Documents
{
	public class DocumentCategoryModel : SecureProjectBoundModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Предшественник"), JsonProperty]
		public virtual DocumentCategoryModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ParentId { get; set; }

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
