using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskAttributesActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskAttributesActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override bool DoProcess(ActivityView view, AttributeChangeActivityRecordModel model)
		{
			return "TaskModel".Equals(model.ItemType) && base.DoProcess(view, model);
		}

		protected override bool DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			return "TaskModel".Equals(model.ItemType) && base.DoProcessItem(view, model);
		}

		protected override void DoPostProcess(ActivityView view)
		{
			base.DoPostProcess(view);

			LocalizeActivityItem<TaskAttributesActivityViewProcessor>(view);
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			if ("DueDate".Equals(view.Action))
				TransformDateValues(view);
			if ("Status".Equals(view.Action))
				LocalizeValuesByType<TaskStatus>(view);
			if ("Priority".Equals(view.Action))
				LocalizeValuesByType<TaskPriority>(view);

			base.DoPostProcessItem(view);

			LocalizeAttribute<TaskModel>(view);
		}

		#endregion
	}
}