using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
    public class TaskCommentsRelatedActivityViewProcessor: RelatedChangeActivityViewProcessor
    {
        public TaskCommentsRelatedActivityViewProcessor(ILocalizationService localizationService) : base(localizationService)
        {
        }

        protected override bool DoProcess(ActivityView view, RelatedChangeActivityRecordModel model)
        {
            return typeof(TaskCommentModel).Name.Equals(model.RelatedItemType) && base.DoProcess(view, model);
        }

        protected override bool DoProcessItem(ActivityItemView view, RelatedChangeActivityRecordModel model)
        {
            return typeof(TaskCommentModel).Name.Equals(model.RelatedItemType) && base.DoProcessItem(view, model);
        }

        protected override void DoPostProcess(ActivityView view)
        {
            base.DoPostProcess(view);

            LocalizeActivityItem<TaskCommentsRelatedActivityViewProcessor>(view);
        }

        protected override void DoPostProcessItem(ActivityItemView view)
        {
            base.DoPostProcessItem(view);

            LocalizeRelatedActivityItem<TaskCommentsRelatedActivityViewProcessor>(view);
        }
    }
}
