using System;
using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Security
{
	/// <summary>
	/// Base class for security providers, contains some boilerplate code, that
	/// potentially will be in each provider implementation
	/// </summary>
	/// <typeparam name="TModel">Model type, that this provider handle</typeparam>
	public abstract class AbstractSecurityConstraintsProvider<TModel>: ISecurityConstraintsProvider
	{
		protected AbstractSecurityConstraintsProvider(IFilteringService filteringService)
		{
			if (filteringService == null)
				throw new ArgumentNullException("filteringService");

			FilteringService = filteringService;
		}

		protected IFilteringService FilteringService { get; private set; }

		public virtual bool AcceptRead(Type modelType, string project, ISession session)
		{
			return typeof (TModel).IsAssignableFrom(modelType);
		}

		public virtual bool AcceptChange(IIdentifiedModel model, string project, ISession session)
		{
			return model != null && AcceptRead(model.GetType(), project, session);
		}

		private UserModel UserFromId(Guid userId, ISession session)
		{
			var user = session.Get<UserModel>(userId);
			if (user == null)
				throw new NoSuchUserException();
			return user;
		}

		public IModelFilterNode ReadConstraint(string project, Guid userId, ISession session)
		{
			return ReadConstraint(project, UserFromId(userId, session), session);
		}

		public bool CanCreate(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanCreate((TModel) model, project, UserFromId(userId, session), session);
		}

		public bool CanUpdate(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanUpdate((TModel)model, project, UserFromId(userId, session), session);
		}

		public bool CanDelete(IIdentifiedModel model, string project, Guid userId, ISession session)
		{
			return CanDelete((TModel)model, project, UserFromId(userId, session), session);
		}

		public abstract IModelFilterNode ReadConstraint(string project, UserModel user, ISession session);

		public abstract bool CanCreate(TModel model, string project, UserModel user, ISession session);

		public abstract bool CanUpdate(TModel model, string project, UserModel user, ISession session);

		public abstract bool CanDelete(TModel model, string project, UserModel user, ISession session);
	}
}