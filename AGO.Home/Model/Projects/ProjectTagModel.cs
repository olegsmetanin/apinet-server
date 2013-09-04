using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Home.Model.Projects
{
	public class ProjectTagModel : TagModel
	{
		#region Persistent

		[DisplayName("Документ"), JsonProperty, NotNull]
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? ProjectId { get; set; }

		#endregion
	}
}
