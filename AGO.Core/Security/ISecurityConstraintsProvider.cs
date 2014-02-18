using System;
using AGO.Core.Filters;
using AGO.Core.Model;
using NHibernate;

namespace AGO.Core.Security
{
	/// <summary>
	/// Base interface for classes, that implements rules of security constraints
	/// on entities for crud operations
	/// </summary>
	public interface ISecurityConstraintsProvider
	{
		/// <summary>
		/// Test that this provider support read (cRud) constraints for models of provided type
		/// </summary>
		/// <param name="modelType">Model type to check</param>
		/// <returns>true, if read access to models can be restricted by this provider</returns>
		bool AcceptRead(Type modelType);

		/// <summary>
		/// Test that this provider support change (CrUD) constraints provided model instance
		/// </summary>
		/// <param name="model">Model to check</param>
		/// <returns>true, if changes to models state can be restricted by this provider</returns>
		bool AcceptChange(IIdentifiedModel model);

		/// <summary>
		/// Implements read constraint
		/// </summary>
		/// <param name="project">Project (may be null for system providers)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for work</param>
		/// <returns>Criteria for read constraint of entity or null, if no restrictions</returns>
		IModelFilterNode ReadConstraint(string project, Guid userId, ISession session);

		/// <summary>
		/// Test, that provided user has permissions to create this model
		/// </summary>
		/// <param name="model">Model to create</param>
		/// <param name="project">Project (may be null for system providers)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for work</param>
		/// <returns>true, if user can create this model (store in db)</returns>
		bool CanCreate(IIdentifiedModel model, string project, Guid userId, ISession session);

		/// <summary>
		/// <see cref="CanCreate"/>
		/// </summary>
		bool CanUpdate(IIdentifiedModel model, string project, Guid userId, ISession session);

		/// <summary>
		/// <see cref="CanCreate"/>
		/// </summary>
		bool CanDelete(IIdentifiedModel model, string project, Guid userId, ISession session);
	}
}