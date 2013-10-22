using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Dictionary
{
	/// <summary>
	/// Тип задачи
	/// </summary>
	public class TaskTypeModel : SecureProjectBoundModel<Guid>, IDictionaryItemModel, ITasksModel
	{
		/// <summary>
		/// Наименование
		/// </summary>
		[JsonProperty, UniqueProperty("ProjectCode"), NotEmpty, NotLonger(256)]
		public virtual string Name { get; set; }
	}
}