using AGO.Core;
using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskAttributeActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskAttributeActivityViewProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService)
			: base(crudDao, sessionProvider, localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override void DoProcess(ActivityView view, AttributeChangeActivityRecordModel model)
		{
			if (!"TaskModel".Equals(model.ItemType))
				return;

			LocalizeActivityItem<TaskAttributeActivityViewProcessor>(view);
		}

		protected override void DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			if (!"TaskModel".Equals(model.ItemType))
				return;

			if ("DueDate".Equals(model.Attribute))
				TransformDateValues(view);

			LocalizeAction<TaskModel>(view);
			LocalizeValues(view);
		}

		#endregion
	}
}