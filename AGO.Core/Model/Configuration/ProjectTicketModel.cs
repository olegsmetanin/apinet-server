using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Core.Model.Configuration
{
	/// <summary>
	/// Ticket for creating project
	/// </summary>
	[RelationalModel]
	public class ProjectTicketModel: IdentifiedModel<Guid>
	{
		/// <summary>
		/// User, that take this ticket
		/// </summary>
		[NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? UserId { get; set; }

		/// <summary>
		/// Project, created by this ticked (or null, if ticket is not used yet)
		/// </summary>
		public virtual ProjectModel Project { get; set; }
		[ReadOnlyProperty]
		public virtual Guid? ProjectId { get; set; }
	}
}
