using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Model.Reporting
{
	/// <summary>
	/// Record of reports archive across all projects
	/// </summary>
	public class ReportArchiveRecordModel: CoreModel<Guid>, IProjectBoundModel
	{
		[NotEmpty, UniqueProperty]
		public virtual Guid ReportTaskId { get; set; }

		[NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE), JsonProperty]
		public virtual string ProjectCode { get; set; }

		[NotEmpty, JsonProperty]
		public virtual string ProjectName { get; set; }

		[NotEmpty, JsonProperty]
		public virtual string ProjectType { get; set; }

		[NotEmpty, NotLonger(250), JsonProperty]
		public virtual string Name { get; set; }

		[NotEmpty, JsonProperty]
		public virtual string SettingsName { get; set; }

		[NotEmpty, JsonProperty]
		public virtual Guid UserId { get; set; }
	}
}
