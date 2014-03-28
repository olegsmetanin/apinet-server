using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class TaskAttributesActivityPostProcessor : AttributeChangeActivityPostProcessor<TaskModel>
	{
		#region Properties, fields, constructors

		public TaskAttributesActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override AttributeChangeActivityRecordModel PopulateActivityRecord(TaskModel model, AttributeChangeActivityRecordModel record, ProjectMemberModel member = null)
		{
			record.ProjectCode = model.ProjectCode;

			return base.PopulateActivityRecord(model, record, member);
		}

		protected override IList<ActivityRecordModel> RecordsForUpdate(TaskModel model, TaskModel original)
		{
			var result = new List<ActivityRecordModel>();

			CheckAttribute(result, model, original, "SeqNumber");
			CheckAttribute(result, model, original, "Status");
			CheckAttribute(result, model, original, "Priority");
			CheckAttribute(result, model, original, "TaskType");
			CheckAttribute(result, model, original, "Content");
			CheckAttribute(result, model, original, "Note");
			CheckAttribute(result, model, original, "DueDate");
			CheckAttribute(result, model, original, "EstimatedTime");
			CheckAttribute(result, model, original, "SpentTime", original.CalculateSpentTime(), model.CalculateSpentTime());

			return result;
		}
		
		#endregion
	}
}
