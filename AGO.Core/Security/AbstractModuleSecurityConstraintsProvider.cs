using System;
using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using NHibernate;

namespace AGO.Core.Security
{
	/// <summary>
	/// Providers in modules operate in project context mostly. This class introduce
	/// project participant as base unit of access restriction (instead of high-level user)
	/// </summary>
	/// <typeparam name="TModel">Model type, that this provider handle</typeparam>
	public abstract class AbstractModuleSecurityConstraintsProvider<TModel> : ISecurityConstraintsProvider
	{
		private readonly ProjectToModuleCache p2m;

		protected AbstractModuleSecurityConstraintsProvider(IFilteringService filteringService, ISessionProviderRegistry providerRegistry)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");
			if (providerRegistry == null)
				throw new ArgumentNullException("providerRegistry");

			FilteringService = filteringService;
// ReSharper disable once DoNotCallOverridableMethodsInConstructor
			p2m = new ProjectToModuleCache(Module, providerRegistry.GetMainDbProvider().SessionFactory);
		}

		protected IFilteringService FilteringService { get; private set; }

		protected ProjectModel CodeToProject(string project, ISession session)
		{
			var p = session.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == project)
				.SingleOrDefault();
			if (p == null)
				throw new NoSuchProjectException();
			return p;
		}
		
		public virtual bool AcceptRead(Type modelType, string project, ISession session)
		{
			return typeof (TModel).IsAssignableFrom(modelType) && p2m.IsProjectInHandledModule(project);
		}

		public virtual bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return model != null && AcceptRead(model.GetType(), project, session);
		}

		protected ProjectMemberModel UserIdToMember(string project, Guid userId, ISession session)
		{
			var member = session.QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project && m.UserId == userId)
				.SingleOrDefault();
			if (member == null)
				throw new NoSuchProjectMemberException();
			return member;
		}

		public IModelFilterNode ReadConstraint(string project, Guid userId, ISession session)
		{
			return ReadConstraint(project, UserIdToMember(project, userId, session), session);
		}

		public bool CanCreate(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanCreate((TModel)model, project, UserIdToMember(project, userId, session), session);
		}

		public bool CanUpdate(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanUpdate((TModel)model, project, UserIdToMember(project, userId, session), session);
		}

		public bool CanDelete(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanDelete((TModel)model, project, UserIdToMember(project, userId, session), session);
		}

		protected abstract string Module { get; }

		public abstract IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session);

		public abstract bool CanCreate(TModel model, string project, ProjectMemberModel member, ISession session);

		public abstract bool CanUpdate(TModel model, string project, ProjectMemberModel member, ISession session);

		public abstract bool CanDelete(TModel model, string project, ProjectMemberModel member, ISession session);
	}
}