using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Projects;

namespace AGO.Core.Controllers.Activity
{
	public class ProjectAttributeActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public ProjectAttributeActivityViewProcessor(
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
			if (!"ProjectModel".Equals(model.ItemType))
				return;

			LocalizeActivityItem<ProjectAttributeActivityViewProcessor>(view);
		}

		protected override void DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			if (!"ProjectModel".Equals(model.ItemType))
				return;

			LocalizeAction<ProjectModel>(view);
			LocalizeValues(view);
		}

		#endregion
	}
}