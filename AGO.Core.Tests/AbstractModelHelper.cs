using System;
using System.Collections.Generic;
using AGO.Core.Model;
using AGO.Core.Model.Activity;
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

		protected virtual ISession ProjectSession(string project)
		{
			return Session();
		}

		public IEnumerable<ProjectMemberModel> ProjectMembers(string code)
		{
			return ProjectSession(code).QueryOver<ProjectMemberModel>().Where(m => m.ProjectCode == code).List();
		}

		public ProjectMemberModel MemberFromUser(string project, UserModel user = null)
		{
			return ProjectSession(project).QueryOver<ProjectMemberModel>()
				.Where(m => m.ProjectCode == project && m.UserId == (user ?? CurrentUser()).Id)
				.SingleOrDefault();
		}

		public void DeleteProjectActivity(string project, ISession session = null)
		{
			session = session ?? Session();
			var activities = session.QueryOver<ActivityRecordModel>()
				.Where(m => m.ProjectCode == project).List();
			foreach (var record in activities)
			{
				session.Delete(record);
			}
		}

		public void DeleteProjectMembers(string project, ISession session = null)
		{
			session = session ?? Session();
			foreach (var member in ProjectMembers(project))
			{
				session.Delete(member);
			}
		}

		public ProjectMembershipModel Membership(string project, UserModel user)
		{
			return Track(() =>
			{
				var p = ProjectFromCode(project);
				var membership = new ProjectMembershipModel
				{
					Project = p,
					User = user
				};
				p.Members.Add(membership);
				Session().Save(membership);
				Session().Flush();
				return membership;
			});
		}

		public ProjectMemberModel Member(string project, UserModel user, params string[] roles)
		{
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentNullException("project");

			var p = ProjectFromCode(project);
			var member = Member(p, user, roles);
			Session().Update(p);
			return member;
		}

		public ProjectMemberModel Member(ProjectModel project, UserModel user, params string[] roles)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (user == null)
				throw new ArgumentNullException("user");

			return Track(() =>
			{
				var membership = new ProjectMembershipModel
				{
					Project = project,
					User = user
				};
				project.Members.Add(membership);
				roles = roles != null && roles.Length > 0 ? roles : new[] { BaseProjectRoles.Administrator };
				var member = ProjectMemberModel.FromParameters(user, project, roles);
				Session().Save(member);
				Session().Flush();
				return member;
			});
		}
	}
}
