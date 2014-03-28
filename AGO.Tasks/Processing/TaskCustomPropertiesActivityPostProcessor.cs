using AGO.Core;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Processing
{
	public class TaskCustomPropertiesActivityPostProcessor : CustomPropertyChangeActivityPostProcessor<TaskCustomPropertyModel>
	{
		#region Properties, fields, constructors

		public TaskCustomPropertiesActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override AttributeChangeActivityRecordModel PopulateActivityRecord(TaskCustomPropertyModel model, AttributeChangeActivityRecordModel record, ProjectMemberModel member = null)
		{
			record = base.PopulateActivityRecord(model, record, member);
			if (model.Task == null)
				return record;

			record.ProjectCode = model.Task.ProjectCode;
			record.ItemName = model.Task.ToStringSafe();
			record.ItemId = model.Task.Id;	

			return record;
		}
		
		#endregion
	}
}
