﻿using AGO.Core;
using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskChangeRelatedActivityViewProcessor : RelatedChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskChangeRelatedActivityViewProcessor(
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
			if (!"TaskModel".Equals(view.ItemType) || typeof(RelatedChangeActivityRecordModel) != view.RecordType)
				return;
			base.DoPostProcessItem(view);

			LocalizeRelatedActivityItem<TaskChangeRelatedActivityViewProcessor>(view);
		}

		#endregion
	}
}