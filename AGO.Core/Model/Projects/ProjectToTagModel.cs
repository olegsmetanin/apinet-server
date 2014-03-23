using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Dictionary.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectToTagModel : CoreModel<Guid>
	{
		[JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[JsonProperty, NotNull]
		public virtual ProjectTagModel Tag { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TagId { get; set; }
	}
}
