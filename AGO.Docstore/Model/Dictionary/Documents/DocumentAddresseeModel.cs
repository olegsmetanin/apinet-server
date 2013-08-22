using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Docstore.Model.Documents;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Dictionary.Documents
{
	public class DocumentAddresseeModel : SecureModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		[DisplayName("Код проекта"), JsonProperty, NotLonger(32), NotEmpty]
		public virtual string ProjectCode { get; set; }

		[DisplayName("Наименование"), JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[DisplayName("Полное наименование"), JsonProperty, NotLonger(1024)]
		public virtual string FullName { get; set; }

		[DisplayName("Предшественник"), JsonProperty]
		public virtual DocumentAddresseeModel Parent { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ParentId { get; set; }

		[DisplayName("Последователи"), PersistentCollection]
		public virtual ISet<DocumentAddresseeModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<DocumentAddresseeModel> _Children = new HashSet<DocumentAddresseeModel>();

		/*[DisplayName("Документы (откуда)"), PersistentCollection(Column = "SenderId")]
		public virtual ISet<DocumentModel> SendingDocuments { get { return _SendingDocuments; } set { _SendingDocuments = value; } }
		private ISet<DocumentModel> _SendingDocuments = new HashSet<DocumentModel>();*/

		[DisplayName("Документы (кому)"), PersistentCollection]
		public virtual ISet<DocumentModel> ReceivingDocuments { get { return _ReceivingDocuments; } set { _ReceivingDocuments = value; } }
		private ISet<DocumentModel> _ReceivingDocuments = new HashSet<DocumentModel>();		

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}