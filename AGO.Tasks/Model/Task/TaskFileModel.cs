using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Files;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Task file model
	/// TODO: extract superclass to core
	/// </summary>
	public class TaskFileModel: SecureModel<Guid>, IFile<TaskModel, TaskFileModel>
	{
		[NotEmpty, NotLonger(256)]
		public virtual string Name { get; set; }

		[NotEmpty, MetadataExclude]
		public virtual string ContentType { get; set; }

		public virtual long Size { get; set; }

		[NotLonger(1024), MetadataExclude]
		public virtual string Path { get; set; }

		public virtual bool Uploaded { get; set; }

		[NotNull]
		public virtual TaskModel Owner { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? OwnerId { get; set; }
	}
}