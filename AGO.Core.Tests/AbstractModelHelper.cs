using System;
using System.Collections.Generic;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Tests
{
	public abstract class AbstractModelHelper
	{
		protected readonly Func<ISession> Session;
		protected readonly Func<UserModel> CurrentUser;
		private readonly Stack<Tuple<Type, Guid>> createdModels;

		protected AbstractModelHelper(Func<ISession> session, Func<UserModel> currentUser)
		{
			Session = session;
			CurrentUser = currentUser;
			createdModels = new Stack<Tuple<Type, Guid>>();
		}

		public T Track<T>(T model) where T : IIdentifiedModel<Guid>
		{
			createdModels.Push(new Tuple<Type, Guid>(model.GetType(), model.Id));
			return model;
		}

		public T Track<T>(Func<T> factory) where T : IIdentifiedModel<Guid>
		{
			return Track(factory());
		}

		public void DropCreated()
		{
			Session().Clear();

			while (createdModels.Count > 0)
			{
				var info = createdModels.Pop();
				var model = Session().Get(info.Item1, info.Item2);
				if (model != null)
					InternalDelete(model);
			}
			Session().Flush();
		}

		protected virtual void InternalDelete(object model)
		{
			Session().Delete(model);
		}

		public ProjectModel ProjectFromCode(string code)
		{
			return Session().QueryOver<ProjectModel>().Where(m => m.ProjectCode == code).SingleOrDefault();
		}

		protected virtual ISession ProjectMembersSession(string project)
		{
			return Session();
		}

		public IEnumerable<ProjectMemberModel> ProjectMembers(string code)
		{
			return ProjectMembersSession(code).QueryOver<ProjectMemberModel>().Where(m => m.ProjectCode == code).List();
		}

		public ProjectMemberModel MemberFromUser(string project, UserModel user = null)
		{
			return ProjectMembersSession(project).QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project && m.UserId == (user ?? CurrentUser()).Id)
				.SingleOrDefault();
		}
	}
}