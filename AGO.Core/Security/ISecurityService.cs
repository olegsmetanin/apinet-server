using System;
using System.Collections.Generic;
using AGO.Core.Filters;
using NHibernate;

namespace AGO.Core.Security
{
	/// <summary>
	/// Register security constraint providers and do CRUD restrictions
	/// base on providers aggregated logic
	/// </summary>
	public interface ISecurityService
	{
		/// <summary>
		/// Register provider, if not registered
		/// </summary>
		/// <param name="provider">Security provider</param>
		/// <exception cref="ArgumentNullException">If provider is null</exception>
		void RegisterProvider(ISecurityConstraintsProvider provider);

		/// <summary>
		/// Calculate read constraints for combination of project, user and model type
		/// </summary>
		/// <typeparam name="T">Type of model</typeparam>
		/// <param name="project">Project (optional for system models)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for access to datastore (if needed)</param>
		/// <param name="criterias">Criterias, that will be combined with security resctriction criterias</param>
		/// <returns>Additional restrictions for security reasons</returns>
		IModelFilterNode ApplyReadConstraint<T>(string project, Guid userId, ISession session, params IModelFilterNode[] criterias);

		/// <summary>
		/// Calculate read constraints for combination of project, user and model type
		/// </summary>
		/// <param name="modelType">Type of model</param>
		/// <param name="project">Project (optional for system models)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for access to datastore (if needed)</param>
		/// <returns>Additional restrictions for security reasons</returns>
		IEnumerable<IModelFilterNode> ApplyReadConstraint(Type modelType, string project, Guid userId, ISession session);
	}
}