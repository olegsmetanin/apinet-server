using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Home.Model.Dictionary.Projects;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
{
	public class ProjectToTagModel : SecureModel<Guid>
	{
		[DisplayName("Проект"), JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		[DisplayName("Тег"), JsonProperty, NotNull]
		public virtual ProjectTagModel Tag { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TagId { get; set; }
	}
}
