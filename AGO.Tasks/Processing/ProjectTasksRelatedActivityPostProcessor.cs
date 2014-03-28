using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class ProjectTasksRelatedActivityPostProcessor : RelatedChangeActivityPostProcessor<TaskModel, ProjectModel>
	{
		#region Properties, fields, constructors

		public ProjectTasksRelatedActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		private RelatedChangeActivityRecordModel FromTask(TaskModel task)
		{
			return new RelatedChangeActivityRecordModel {ProjectCode = task.ProjectCode};
		}

		protected override IList<ActivityRecordModel> RecordsForInsertion(TaskModel model)
		{
			var result = new List<ActivityRecordModel>();
			
			var project = SessionProviderRegistry.GetMainDbProvider().CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == model.ProjectCode).SingleOrDefault();
			if(project != null)
				result.Add(PopulateRelatedActivityRecord(model, project, FromTask(model), ChangeType.Insert));

			return result;
		} 

		protected override IList<ActivityRecordModel> RecordsForDeletion(TaskModel model)
		{
			var result = new List<ActivityRecordModel>();

			var project = SessionProviderRegistry.GetMainDbProvider().CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == model.ProjectCode).SingleOrDefault();
			if (project != null)
				result.Add(PopulateRelatedActivityRecord(model, project, FromTask(model), ChangeType.Delete));

			return result;
		}

		#endregion
	}
}
