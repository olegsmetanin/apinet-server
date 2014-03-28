using System;
using System.Collections.Generic;
using AGO.Core.Model.Projects;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary
{
	[TablePerSubclass("ModelType")]
	public class TagModel : CoreModel<Guid>, IHierarchicalDictionaryItemModel
	{
		#region Persistent

		/// <summary>
		/// UserId of tag owner (can't use proj member entity, because may be used in project too)
		/// </summary>
		[MetadataExclude]
		public virtual Guid OwnerId { get; set; }

		[JsonProperty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

		[JsonProperty, NotLonger(128), NotEmpty]
		public virtual string Name { get; set; }

		[JsonProperty, NotLonger(1024), UniqueProperty("ProjectCode", "OwnerId")]
		public virtual string FullName { get; set; }

		[JsonProperty]
		public virtual TagModel Parent { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ParentId { get; set; }

		[PersistentCollection(Column = "ParentId")]
		public virtual ISet<TagModel> Children { get { return _Children; } set { _Children = value; } }
		private ISet<TagModel> _Children = new HashSet<TagModel>();

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		[NotMapped, MetadataExclude]
		public virtual int Level
		{
			get { return Parent != null ? Parent.Level + 1 : 1; }
		}

		#endregion
	}
}