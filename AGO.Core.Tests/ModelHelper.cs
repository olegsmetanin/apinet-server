using System;
using System.Linq;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Tests
{
	public class ModelHelper: AbstractModelHelper
	{
		public ModelHelper(Func<ISession> session, Func<UserModel> currentUser):base(session, currentUser)
		{
		}

		protected override void InternalDelete(object model)
		{
			var project = model as ProjectModel;
			if (project != null)
			{
				foreach (var member in ProjectMembers(project.ProjectCode))
				{
					Session().Delete(member);
				}
				DeleteProjectActivity(project.ProjectCode);
			}
			base.InternalDelete(model);
		}

		public ProjectTypeModel ProjectType(string name = null)
		{
			return Track(() =>
			{
				var pt = new ProjectTypeModel
				{
					CreationTime = DateTime.UtcNow,
					Module = "NUnit",
					Name = name ?? "NUnit test project type"
				};
				Session().Save(pt);
				Session().Flush();
				return pt;
			});
		}

		public ProjectModel Project(string code, string type = null, string name = null, bool pub = false)
		{
			if (code.IsNullOrWhiteSpace())
				throw new ArgumentNullException("code");

			return Track(() =>
			{
				var pt = !type.IsNullOrWhiteSpace()
					? Session().QueryOver<ProjectTypeModel>().Where(m => m.Name == type).SingleOrDefault()
					: Session().QueryOver<ProjectTypeModel>().List().Take(1).First();
				var p = new ProjectModel
				{
					CreationTime = DateTime.UtcNow,
					ProjectCode = code,
					Name = name ?? "NUnit project " + code,
					Type = pt,
					VisibleForAll = pub,
					ConnectionString = Session().Connection.ConnectionString //by default use main cs
				};
				Session().Save(p);
				Session().Flush();
				return p;
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

		public ProjectTagModel ProjectTag(string name = null, UserModel owner = null)
		{
			return Track(() =>
			{
				var tag = new ProjectTagModel
				{
					OwnerId = (owner ?? CurrentUser()).Id,
					CreationTime = DateTime.UtcNow,
					Name = name ?? "NUnit test tag"
				};
				tag.FullName = tag.Name;
				Session().Save(tag);
				Session().Flush();
				return tag;
			});
		}
	}
}