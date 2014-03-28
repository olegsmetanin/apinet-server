using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class TaskExecutorsRelatedActivityPostProcessor : RelatedChangeActivityPostProcessor<TaskExecutorModel, TaskModel>
	{
		#region Properties, fields, constructors

		public TaskExecutorsRelatedActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override IList<ActivityRecordModel> RecordsForInsertion(TaskExecutorModel model)
		{
			var result = new List<ActivityRecordModel>();
			if (model.Task == null)
				return result;
			
			result.Add(PopulateRelatedActivityRecord(model, model.Task,
				new RelatedChangeActivityRecordModel {ProjectCode = model.Task.ProjectCode},  ChangeType.Insert));

			return result;
		} 

		protected override IList<ActivityRecordModel> RecordsForDeletion(TaskExecutorModel model)
		{
			var result = new List<ActivityRecordModel>();
			if (model.Task == null)
				return result;

			result.Add(PopulateRelatedActivityRecord(model, model.Task,
				new RelatedChangeActivityRecordModel {ProjectCode = model.Task.ProjectCode},  ChangeType.Delete));

			return result;
		}

		#endregion
	}
}
