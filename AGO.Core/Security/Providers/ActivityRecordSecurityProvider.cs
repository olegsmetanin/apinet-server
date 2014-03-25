using System;
using AGO.Core.Filters;
using AGO.Core.Model.Activity;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security.Providers
{
	public class ActivityRecordSecurityProvider: AbstractSecurityConstraintsProvider<ActivityRecordModel>
	{
		private readonly ISessionFactory mainFactory;

		public ActivityRecordSecurityProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
			: base(filteringService)
		{
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			mainFactory = providerRegistry.GetMainDbProvider().SessionFactory;
		}

		//because this provider always used in modules, but logic same in each module, we implement it in core
		//we can't use project session to obtain user and use main session for this
		//may be this situation changed in modules will have own logic for restrict access to activity records
		protected override UserModel UserFromId(Guid userId, ISession session)
		{
			var s = mainFactory.OpenSession();
			try
			{
				return base.UserFromId(userId, s);
			}
			finally
			{
				s.Close();
			}
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