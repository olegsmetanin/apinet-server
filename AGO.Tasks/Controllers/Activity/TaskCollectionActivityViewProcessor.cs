using AGO.Core;
using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskCollectionActivityViewProcessor : CollectionChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskCollectionActivityViewProcessor(
			ICrudDao crudDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService)
			: base(crudDao, sessionProvider, localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override void DoProcess(ActivityView view, CollectionChangeActivityRecordModel model)
		{
			LocalizeActivityItem<TaskCollectionActivityViewProcessor>(view);
		}

		protected override void DoProcessItem(ActivityItemView view, CollectionChangeActivityRecordModel model)
		{
			if (!"TaskModel".Equals(model.RelatedItemType))
				return;

			LocalizeRelatedActivityItem<TaskCollectionActivityViewProcessor>(view);
		}

		#endregion
	}
}