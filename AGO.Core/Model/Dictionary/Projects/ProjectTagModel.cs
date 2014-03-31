using System.Collections.Generic;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Projects;

namespace AGO.Core.Model.Dictionary.Projects
{
	public class ProjectTagModel : TagModel
	{
		public static readonly string TypeCode = ModuleDescriptor.MODULE_CODE + ".project";

		[PersistentCollection(CascadeType = CascadeType.Delete, Column = "TagId")]
		public virtual ISet<ProjectToTagModel> ProjectLinks { get { return links; } set { links = value; } }
		private ISet<ProjectToTagModel> links = new HashSet<ProjectToTagModel>();
	}
}
