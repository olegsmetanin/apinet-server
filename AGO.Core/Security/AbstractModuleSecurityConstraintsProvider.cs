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
		protected AbstractModuleSecurityConstraintsProvider(IFilteringService filteringService)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");

			FilteringService = filteringService;
		}

		protected IFilteringService FilteringService { get; private set; }

		public bool AcceptRead(Type modelType)
		{
			return typeof (TModel).IsAssignableFrom(modelType);
		}

		public bool AcceptChange(IIdentifiedModel model)
		{
			return model != null && AcceptRead(model.GetType());
		}

		private ProjectMemberModel UserIdToMember(string project, Guid userId, ISession session)
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

		public abstract IModelFilterNode ReadConstraint(string project, ProjectMemberModel member, ISession session);

		public abstract bool CanCreate(TModel model, string project, ProjectMemberModel member, ISession session);

		public abstract bool CanUpdate(TModel model, string project, ProjectMemberModel member, ISession session);

		public abstract bool CanDelete(TModel model, string project, ProjectMemberModel member, ISession session);
	}
}