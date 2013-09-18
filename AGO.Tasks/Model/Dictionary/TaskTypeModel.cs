using System;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Dictionary
{
	/// <summary>
	/// Тип задачи
	/// </summary>
	[Table(SchemaName = "Tasks")]
	public class TaskTypeModel: SecureModel<Guid>, IDictionaryItemModel
	{
		/// <summary>
		/// Код проекта
		/// </summary>
		[DisplayName("Код проекта"), JsonProperty, NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		[DisplayName("Наименование"), JsonProperty, NotEmpty, NotLonger(256)]
		public virtual string Name { get; set; }
	}
}