using System;
using System.Linq;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Core.Model.Projects
{
	public class ProjectMemberModel : CoreModel<Guid>, IProjectBoundModel
	{
		#region Persistent
		
		[JsonProperty, NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

		[JsonProperty, NotEmpty]
		public virtual Guid UserId { get; set; }

		[JsonProperty, NotEmpty, NotLonger(UserModel.FULLNAME_SIZE)]
		public virtual string FullName { get; set; }

		[MetadataExclude, NotEmpty, NotLonger(128)]
		public virtual string RolesString { get; set; }

		[JsonProperty, NotEmpty, NotLonger(128)]
		public virtual string CurrentRole { get; set; }

		public virtual int UserPriority { get; set; }

		#endregion

		[NotMapped, MetadataExclude]
		public virtual string[] Roles
		{
			get { return RolesString != null ? RolesString.Split(' ') : new string[0]; }
			set { RolesString = value != null ? string.Join(" ", value) : null; }
		}

		/// <summary>
		/// Project members has <paramref name="role"/> assigned to him
		/// </summary>
		/// <param name="role">Searched role</param>
		/// <returns>true, if project member has one of him roles equal to searched</returns>
		public virtual bool HasRole(string role)
		{
			return !string.IsNullOrWhiteSpace(role) &&
			       Roles.Any(r => role.Equals(r, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Project member currently used role equal to searched <paramref name="role"/>
		/// </summary>
		/// <param name="role">Searched role</param>
		/// <returns>true, if current project member role equal to searched</returns>
		public virtual bool IsInRole(string role)
		{
			return !string.IsNullOrWhiteSpace(role) && !string.IsNullOrWhiteSpace(CurrentRole) &&
				   CurrentRole.Equals(role, StringComparison.InvariantCultureIgnoreCase);
		}

		public static ProjectMemberModel FromParameters(UserModel user, ProjectModel project, params string[] roles)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (project == null)
				throw new ArgumentNullException("project");
			if (roles == null || roles.Length <= 0)
				throw new ArgumentException("No rules defined for project member", "roles");

			return new ProjectMemberModel
			{
				ProjectCode = project.ProjectCode,
				UserId = user.Id,
				FullName = user.FullName,
				Roles = roles,
				CurrentRole = roles[0]
			};
		}

		public override string ToString()
		{
			return FullName.IsNullOrWhiteSpace() ? base.ToString() : FullName.TrimSafe();
		}
	}
}
