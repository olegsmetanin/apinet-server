using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskAttributeActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskAttributeActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override void DoProcess(ActivityView view, AttributeChangeActivityRecordModel model)
		{
			if (!"TaskModel".Equals(view.ItemType))
				return;
			base.DoProcess(view, model);
		}

		protected override void DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			if (!"TaskModel".Equals(view.ItemType))
				return;
			base.DoProcessItem(view, model);
		}

		protected override void DoPostProcess(ActivityView view)
		{
			if (!"TaskModel".Equals(view.ItemType))
				return;
			base.DoPostProcess(view);

			LocalizeActivityItem<TaskAttributeActivityViewProcessor>(view);
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			if (!"TaskModel".Equals(view.ItemType) || typeof(AttributeChangeActivityRecordModel) != view.RecordType)
				return;

			if ("DueDate".Equals(view.Action))
				TransformDateValues(view);

			base.DoPostProcessItem(view);
			LocalizeAction<TaskModel>(view);
		}

		#endregion
	}
}