using System.Collections.Generic;
using AGO.Core.DataAccess;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;

namespace AGO.Tasks.Processing
{
	public class ProjectAttributesActivityPostProcessor : AttributeChangeActivityPostProcessor<ProjectModel>
	{
		#region Properties, fields, constructors

		public ProjectAttributesActivityPostProcessor(
			DaoFactory factory,
			ISessionProviderRegistry providerRegistry)
			: base(factory, providerRegistry)
		{
		}

		#endregion

		#region Template methods

		protected override AttributeChangeActivityRecordModel PopulateActivityRecord(ProjectModel model, AttributeChangeActivityRecordModel record, ProjectMemberModel member = null)
		{
			record.ProjectCode = model.ProjectCode;

			return base.PopulateActivityRecord(model, record, member);
		}

		protected override IList<ActivityRecordModel> RecordsForUpdate(ProjectModel model, ProjectModel original)
		{
			var result = new List<ActivityRecordModel>();

			CheckAttribute(result, model, original, "Name");
			CheckAttribute(result, model, original, "Description");
			CheckAttribute(result, model, original, "Type");
			CheckAttribute(result, model, original, "VisibleForAll");
			CheckAttribute(result, model, original, "Status");

			return result;
		}
		
		#endregion
	}
}
