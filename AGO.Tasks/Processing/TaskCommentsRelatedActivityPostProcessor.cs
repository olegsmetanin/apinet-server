using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
    public class TaskCommentsRelatedActivityPostProcessor: RelatedChangeActivityPostProcessor<TaskCommentModel, TaskModel>
    {
        public TaskCommentsRelatedActivityPostProcessor(DaoFactory factory, ISessionProviderRegistry providerRegistry) : base(factory, providerRegistry)
        {
        }

        //Comments only added, not updated or deleted
        protected override IList<ActivityRecordModel> RecordsForInsertion(TaskCommentModel model)
        {
            var result = new List<ActivityRecordModel>();
            if (model.Task == null)
                return result;

            result.Add(PopulateRelatedActivityRecord(model, model.Task,
                new RelatedChangeActivityRecordModel { ProjectCode = model.Task.ProjectCode }, ChangeType.Insert));

            return result;
        } 
    }
}
