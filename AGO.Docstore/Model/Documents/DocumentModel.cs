using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Docstore.Model.Dictionary;
using AGO.Docstore.Model.Security;
using AGO.Hibernate.Attributes.Constraints;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Docstore.Model.Documents
{
	public class DocumentModel : SecureModel<Guid>
	{
		#region Persistent

		[DisplayName("Номер п/п"), JsonProperty, NotEmpty, NotLonger(16)]
		public virtual string SeqNumber { get; set; }

		[DisplayName("Тип документа"), JsonProperty, EnumDisplayNames(new[]
		{
			"Incoming", "Входящие",
			"Outgoing", "Исходящие",
			"Internal", "Внутренние"
		})]
		public virtual DocumentType DocumentType { get; set; }

		[DisplayName("Краткое содержание"), NotEmpty, NotLonger(256), JsonProperty]
		public virtual string Annotation { get; set; }

		[DisplayName("Содержание"), JsonProperty]
		public virtual string Content { get; set; }	

		[DisplayName("Дата документа"), JsonProperty, NotNull]
		public virtual DateTime? Date { get; set; }

		[DisplayName("Номер документа"), JsonProperty, NotEmpty, NotLonger(64)]
		public virtual string Number { get; set; }

		[DisplayName("Url исходного документа"), JsonProperty, NotLonger(512)]
		public virtual string SourceDocUrl { get; set; }

		[DisplayName("Дата исходного документа"), JsonProperty]
		public virtual DateTime? SourceDocDate { get; set; }

		[DisplayName("Номер исходного документа"), JsonProperty, NotLonger(64)]
		public virtual string SourceDocNumber { get; set; }

		[DisplayName("Статус"), /*JsonProperty,*/ NotNull]
		public virtual DocumentStatusModel Status { get; set; }

		[DisplayName("История статусов документа"), PersistentCollection]
		public virtual ISet<DocumentStatusHistoryModel> StatusHistory { get { return _StatusHistory; } set { _StatusHistory = value; } }
		private ISet<DocumentStatusHistoryModel> _StatusHistory = new HashSet<DocumentStatusHistoryModel>();

		[DisplayName("Категории документов"), PersistentCollection(Inverse = false)]
		public virtual ISet<DocumentCategoryModel> Categories { get { return _Categories; } set { _Categories = value; } }
		private ISet<DocumentCategoryModel> _Categories = new HashSet<DocumentCategoryModel>();

		[DisplayName("Комментарии"), PersistentCollection]
		public virtual ISet<DocumentCommentModel> Comments { get { return _Comments; } set { _Comments = value; } }
		private ISet<DocumentCommentModel> _Comments = new HashSet<DocumentCommentModel>();

		/*[DisplayName("Адресат (от кого)")]
		public virtual DocumentAddresseeModel Sender { get; set; }*/

		[DisplayName("Адресаты (кому)"), PersistentCollection(Inverse = false)]
		public virtual ISet<DocumentAddresseeModel> Receivers { get { return _Receivers; } set { _Receivers = value; } }
		private ISet<DocumentAddresseeModel> _Receivers = new HashSet<DocumentAddresseeModel>();

		[DisplayName("Параметры"), PersistentCollection]
		public virtual ISet<DocumentCustomPropertyModel> CustomProperties { get { return _CustomProperties; } set { _CustomProperties = value; } }
		private ISet<DocumentCustomPropertyModel> _CustomProperties = new HashSet<DocumentCustomPropertyModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Annotation;
		}
		
		#endregion
	}
}