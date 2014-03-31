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

		public ProjectTagModel ProjectTag(string name = null, UserModel owner = null, ProjectTagModel parent = null)
		{
			return Track(() =>
			{
				var tag = new ProjectTagModel
				{
					OwnerId = (owner ?? CurrentUser()).Id,
					CreationTime = DateTime.UtcNow,
					Name = name ?? "NUnit test tag",
					Parent = parent
				};
				tag.FullName = tag.Name;
				Session().Save(tag);
				Session().Flush();
				return tag;
			});
		}
	}
}