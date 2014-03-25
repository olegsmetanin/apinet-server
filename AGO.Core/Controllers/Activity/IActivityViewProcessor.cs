using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	public interface IActivityViewProcessor
	{
		void Process(ActivityView view, ActivityRecordModel model);

		void PostProcess(ActivityView view);

		void ProcessItem(ActivityItemView view, ActivityRecordModel model);

		void PostProcessItem(ActivityItemView view);
	}
}