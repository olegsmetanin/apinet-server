using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class TaskCollectionActivityPostProcessor : CollectionChangeActivityPostProcessor<TaskModel, TaskModel>
	{
		#region Properties, fields, constructors

		public TaskCollectionActivityPostProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider)
			: base(crudDao, sessionProvider)
		{
		}

		#endregion

		#region Template methods

		protected override IList<ActivityRecordModel> RecordsForInsertion(TaskModel model)
		{
			var result = new List<ActivityRecordModel>();
			
			var project = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == model.ProjectCode).Take(1).SingleOrDefault();
			if(project != null)
					result.Add(PopulateCollectionActivityRecord(model, model,
				new CollectionChangeActivityRecordModel { ProjectCode = model.ProjectCode}, ChangeType.Insert));

			return result;
		} 

		protected override IList<ActivityRecordModel> RecordsForDeletion(TaskModel model)
		{
			var result = new List<ActivityRecordModel>();

			var project = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == model.ProjectCode).Take(1).SingleOrDefault();
			if (project != null)
					result.Add(PopulateCollectionActivityRecord(model, model,
				new CollectionChangeActivityRecordModel { ProjectCode = model.ProjectCode }, ChangeType.Delete));

			return result;
		}

		#endregion
	}
}
