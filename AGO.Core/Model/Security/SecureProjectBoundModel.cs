using AGO.Core.Attributes.Constraints;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public abstract class SecureProjectBoundModel<TIdType>: SecureModel<TIdType>, IProjectBoundModel
	{
		[JsonProperty, NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }
	}
}