using System.Collections.Generic;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Projects;

namespace AGO.Core.Model.Dictionary.Projects
{
	public class ProjectTagModel : TagModel
	{
		[PersistentCollection(CascadeType = CascadeType.Delete, Column = "TagId")]
		public virtual ISet<ProjectToTagModel> Tags { get { return _Tags; } set { _Tags = value; } }
		private ISet<ProjectToTagModel> _Tags = new HashSet<ProjectToTagModel>();
	}
}
