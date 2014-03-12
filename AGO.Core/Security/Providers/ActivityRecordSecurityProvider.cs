using AGO.Core.Filters;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ActivityRecordSecurityProvider: AbstractSecurityConstraintsProvider<ActivityRecordModel>
	{
		public ActivityRecordSecurityProvider(IFilteringService filteringService)
			: base(filteringService)
		{
		}

		public override IModelFilterNode ReadConstraint(string project, UserModel user, ISession session)
		{
			return FilteringService.Filter<ActivityRecordModel>()
				.Where(m => m.ProjectCode == project);
		}

		public override bool CanCreate(ActivityRecordModel model, string project, UserModel user, ISession session)
		{
			return true;
		}

		public override bool CanUpdate(ActivityRecordModel model, string project, UserModel user, ISession session)
		{
			return true;
		}

		public override bool CanDelete(ActivityRecordModel model, string project, UserModel user, ISession session)
		{
			return true;
		}
	}
}