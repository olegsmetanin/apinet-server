using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Projects;

namespace AGO.Tasks.Controllers.Activity
{
	public class ProjectAttributesActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public ProjectAttributesActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override bool DoProcess(ActivityView view, AttributeChangeActivityRecordModel model)
		{
			return "ProjectModel".Equals(model.ItemType) && base.DoProcess(view, model);
		}

		protected override bool DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			return "ProjectModel".Equals(model.ItemType) && base.DoProcessItem(view, model);
		}

		protected override void DoPostProcess(ActivityView view)
		{
			base.DoPostProcess(view);

			LocalizeActivityItem<ProjectAttributesActivityViewProcessor>(view);
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			if ("Status".Equals(view.Action))
				LocalizeValuesByType<ProjectStatus>(view);

			base.DoPostProcessItem(view);

			LocalizeAttribute<ProjectModel>(view);
		}

		#endregion
	}
}