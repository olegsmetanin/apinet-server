using AGO.Core.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public abstract class SecureProjectBoundModel<TIdType>: SecureModel<TIdType>, IProjectBoundModel
	{
		[JsonProperty, NotEmpty, NotLonger(32)]
		public virtual string ProjectCode { get; set; }
	}
}