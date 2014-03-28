using System;
using AGO.Core.Controllers.Activity;
using AGO.Core.Localization;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Dictionary;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.Activity
{
	public class TaskCustomPropertiesActivityViewProcessor : AttributeChangeActivityViewProcessor
	{
		#region Properties, fields, constructors
		public TaskCustomPropertiesActivityViewProcessor(ILocalizationService localizationService)
			: base(localizationService)
		{		
		}

		#endregion

		#region Template methods

		protected override bool DoProcess(ActivityView view, AttributeChangeActivityRecordModel model)
		{
			return "TaskCustomPropertyModel".Equals(model.ItemType) && base.DoProcess(view, model);
		}

		protected override bool DoProcessItem(ActivityItemView view, AttributeChangeActivityRecordModel model)
		{
			return "TaskCustomPropertyModel".Equals(model.ItemType) && base.DoProcessItem(view, model);
		}

		protected override void DoPostProcess(ActivityView view)
		{
			base.DoPostProcess(view);

			LocalizeActivityItem<TaskCustomPropertiesActivityViewProcessor>(view);
		}

		protected override void DoPostProcessItem(ActivityItemView view)
		{
			if (CustomPropertyValueType.Date.ToString().Equals(view.AdditionalInfo))
				TransformDateValues(view);	

			base.DoPostProcessItem(view);

			LocalizeAttribute<TaskCustomPropertyModel>(view);
		}

		protected override string GetLocalizedAttributeName(Type type, string attribute)
		{
			return attribute;
		}

		#endregion
	}
}