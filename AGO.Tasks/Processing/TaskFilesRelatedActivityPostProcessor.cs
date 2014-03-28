using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class TaskFilesRelatedActivityPostProcessor : RelatedChangeActivityPostProcessor<TaskFileModel, TaskModel>
	{
		#region Properties, fields, constructors

		public TaskFilesRelatedActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override IList<ActivityRecordModel> RecordsForInsertion(TaskFileModel model)
		{
			var result = new List<ActivityRecordModel>();
			if (model.Owner == null)
				return result;

			result.Add(PopulateRelatedActivityRecord(model, model.Owner,
				new RelatedChangeActivityRecordModel { ProjectCode = model.Owner.ProjectCode }, ChangeType.Insert));

			return result;
		} 

		protected override IList<ActivityRecordModel> RecordsForDeletion(TaskFileModel model)
		{
			var result = new List<ActivityRecordModel>();
			if (model.Owner == null)
				return result;

			result.Add(PopulateRelatedActivityRecord(model, model.Owner,
				new RelatedChangeActivityRecordModel { ProjectCode = model.Owner.ProjectCode }, ChangeType.Delete));

			return result;
		}

		#endregion
	}
}
