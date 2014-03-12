using AGO.Core.Model.Activity;

namespace AGO.Core.Controllers.Activity
{
	public interface IActivityViewProcessor
	{
		void Process(ActivityView view, ActivityRecordModel model);

		void ProcessItem(ActivityItemView view, ActivityRecordModel model);
	}
}