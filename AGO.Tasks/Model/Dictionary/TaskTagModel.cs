using System.Collections.Generic;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Model.Dictionary;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Model.Dictionary
{
	/// <summary>
	/// Модель тега задачи
	/// </summary>
	public class TaskTagModel: TagModel, ITasksModel
	{
		public static readonly string TypeCode = ModuleDescriptor.MODULE_CODE + ".task";

		/// <summary>
		/// Связи с задачами (надо удалить при удалении тегов)
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan, Column = "TagId")]
		public virtual ISet<TaskToTagModel> TaskLinks
		{
			get { return linksStore; }
			set { linksStore = value; }
		}
		private ISet<TaskToTagModel> linksStore = new HashSet<TaskToTagModel>();
	}
}