using AGO.Core;
using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;

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

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			if (!"TaskModel".Equals(view.ItemType) || typeof(CollectionChangeActivityRecordModel) != view.RecordType)
				return;
			base.DoPostProcessItem(view);

			LocalizeRelatedActivityItem<TaskCollectionActivityViewProcessor>(view);
		}

		#endregion
	}
}