using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Filters;
using NHibernate;


namespace AGO.Core.Security
{
	public class SecurityService: ISecurityService
	{
		private readonly List<ISecurityConstraintsProvider> providers;
		private readonly IFilteringService fs;

		public SecurityService(IFilteringService filteringService)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");

			fs = filteringService;
			providers = new List<ISecurityConstraintsProvider>();
		}

		public void RegisterProvider(ISecurityConstraintsProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (!providers.Contains(provider))
				providers.Add(provider);
		}

		public IModelFilterNode ApplyReadConstraint<T>(string project, Guid userId, ISession session, params IModelFilterNode[] criterias)
		{
			var restrictions = providers
				.Where(p => p.AcceptRead(typeof(T)))
				.Select(p => p.ReadConstraint(project, userId, session));

			if (criterias != null && criterias.Length > 0)
				restrictions = restrictions.Concat(criterias);

			return fs.ConcatFilters(restrictions);
		}

		public IEnumerable<IModelFilterNode> ApplyReadConstraint(Type modelType, string project, Guid userId, ISession session)
		{
			return providers
				.Where(p => p.AcceptRead(modelType))
				.Select(p => p.ReadConstraint(project, userId, session));
		}

//		public IModelFilterNode ApplyReadConstraint<T>(string project, Guid userId, ISession session)
//		{
//			throw new NotImplementedException();
//			return providers
//				.Where(p => p.AcceptRead(typeof (T)))
//				.Aggregate(criteria, (current, provider) =>
//				{
//					var restriction = provider.ReadConstraint(project, userId, session);
//					return restriction != null ? criteria.And(restriction) : criteria;
//				});

//			return ApplyReadConstraint(typeof (T), criteria, project, userId, session) as IQueryOver<T>;
//		}

//		public IQueryOver ApplyReadConstraint(Type modelType, IQueryOver criteria, string project, Guid userId, ISession session)
//		{
//			criteria.w
//
//			providers.Where(p => p.AcceptRead(modelType))
//				.Aggregate(criteria, (current, provider) =>
//				{
//					var restriction = provider.ReadConstraint(project, userId, session);
//					return restriction != null ? current.
//				} )
//		}
	}
}