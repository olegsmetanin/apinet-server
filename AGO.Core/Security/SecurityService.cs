using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model;
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
				.Where(p => p.AcceptRead(typeof(T), project, session))
				.Select(p => p.ReadConstraint(project, userId, session));

			if (criterias != null && criterias.Length > 0)
				restrictions = restrictions.Concat(criterias);

			return fs.ConcatFilters(restrictions);
		}

		public IEnumerable<IModelFilterNode> ApplyReadConstraint(Type modelType, string project, Guid userId, ISession session)
		{
			return providers
				.Where(p => p.AcceptRead(modelType, project, session))
				.Select(p => p.ReadConstraint(project, userId, session));
		}

		public void DemandUpdate(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var isNew = model.IsNew();
			var allowed = providers
				.Where(p => p.AcceptChange(model, project, session))
				.All(p => isNew 
					? p.CanCreate(model, project, userId, session) 
					: p.CanUpdate(model, project, userId, session));
			if (allowed) return;

			if (isNew)
				throw new CreationDeniedException(model);

			throw new ChangeDeniedException(model);
		}

		public void DemandDelete(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var allowed = providers
				.Where(p => p.AcceptChange(model, project, session))
				.All(p => p.CanDelete(model, project, userId, session));
			if (!allowed)
				throw new DeleteDeniedException(model);
		}
	}
}