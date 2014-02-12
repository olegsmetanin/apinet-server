using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Localization;
using AGO.Core.Model.Security;

namespace AGO.Tasks
{
	/// <summary>
	/// Constants for task project type
	/// </summary>
	public sealed class TaskProjectRoles: BaseProjectRoles
	{
		public static readonly string Manager = "Manager";
		public static readonly string Executor = "Executor";

		/// <summary>
		/// Check that supplied rules is not empty and in restricted set of values
		/// </summary>
		public static bool IsValid(params string[] roles)
		{
			if (roles == null || roles.Length <= 0) return false;

			return roles.All(r => r != null && (r == Administrator || r == Manager || r == Executor));
		}

		public static LookupEntry[] Roles(ILocalizationService ls)
		{
			return Roles(ls, Administrator, Manager, Executor);
		}

		public static LookupEntry[] Roles(ILocalizationService ls, params string[] roles)
		{
			if (roles == null || roles.Length <= 0) return new LookupEntry[0];

			return roles.Select(r => RoleToLookupEntry(r, ls)).ToArray();
		}

		public static LookupEntry RoleToLookupEntry(string role, ILocalizationService ls)
		{
			if (role.IsNullOrWhiteSpace())
				throw new ArgumentNullException("role");
			if (ls == null)
				throw new ArgumentNullException("ls");

			return new LookupEntry
			{
				Id = role,
				Text = ls.MessageForType(typeof (BaseProjectRoles), role) 
					?? ls.MessageForType(typeof (TaskProjectRoles), role) 
					?? role
			};
		}
	}
}