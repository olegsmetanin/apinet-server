using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Model of task commentary
    /// </summary>
    public class TaskCommentModel: SecureModel<Guid>
    {
        [NotNull]
        public virtual TaskModel Task { get; set; }
        [ReadOnlyProperty, MetadataExclude]
        public virtual Guid? TaskId { get; set; }

        [NotEmpty]
        public virtual string Text { get; set; } 
    }
}
