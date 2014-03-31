using System;
using System.Linq;
using System.Reflection;
using AGO.Core.Application;
using AGO.Core.Filters;
using AGO.Core.Model;
using NHibernate;
using SimpleInjector;
using SimpleInjector.Advanced;

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
		/// <param name="criterias">Criterias, that will be combined with security resctriction criterias</param>
		/// <returns>Additional restrictions for security reasons</returns>
		IModelFilterNode ApplyReadConstraint(Type modelType, string project, Guid userId, ISession session, params IModelFilterNode[] criterias);

		/// <summary>
		/// Test, that current user has permissions (does't have restrictons) for
		/// updating provided model instance. For successfully update, all registered providers,
		/// that accept this model, must return true for HasPermission request.
		/// </summary>
		/// <param name="model">Model for test</param>
		/// <param name="project">Project(optional for system models)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for access to datastore (if needed)</param>
		/// <exception cref="ArgumentNullException">Throwed, if model is null</exception>
		/// <exception cref="CreationDeniedException">Throwed if not enough permissions for create new model</exception>
		/// <exception cref="ChangeDeniedException">Throwed if not enough permissions for update model</exception>
		void DemandUpdate(IIdentifiedModel model, string project, Guid userId, ISession session);

		/// <summary>
		/// Test, that current user has permissions (does't have restrictons) for
		/// deleting provided model instance. For successfully delete, all registered providers,
		/// that accept this model, must return true for HasPermission request.
		/// </summary>
		/// <param name="model">Model for test</param>
		/// <param name="project">Project(optional for system models)</param>
		/// <param name="userId">User identifier</param>
		/// <param name="session">NHibernate session for access to datastore (if needed)</param>
		/// <exception cref="ArgumentNullException">Throwed, if model is null</exception>
		/// <exception cref="DeleteDeniedException">Throwed if not enough permissions for delete model</exception>
		void DemandDelete(IIdentifiedModel model, string project, Guid userId, ISession session);
	}

	public static class SecurityServiceExtensions
	{
		/// <summary>
		/// Register all implementations of <see cref="ISecurityConstraintsProvider"/> in ioc container (because
		/// implementations may have its own required dependencies). Must be called in <see cref="ModuleDescriptor.Register"/>
		/// of each module, that contains security providers.
		/// </summary>
		/// <param name="app">Application instance</param>
		/// <param name="assembly">Module assembly, where implementations will be searched (via GetExportedTypes)</param>
		/// <example>
		/// public void Register(IApplication app)
		/// {
		///     app.RegisterModuleSecurityProviders(GetType().Assembly);
		/// }
		/// </example>
		public static void RegisterModuleSecurityProviders(this IApplication app, Assembly assembly)
		{
			//can't use ISesurityService at this stage
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			var scpt = typeof (ISecurityConstraintsProvider);
			var securityProviderImplTypes = assembly.GetExportedTypes()
				.Where(type => type.IsClass && !type.IsAbstract && scpt.IsAssignableFrom(type));
			foreach (var providerType in securityProviderImplTypes)
			{
				//Lifestyle - may be transient more adequacy?
				app.IocContainer.AppendToCollection(scpt, 
					Lifestyle.Singleton.CreateRegistration(scpt, providerType, app.IocContainer)); 
			}
		}

		/// <summary>
		/// Register all security providers, that found in ioc container at this point, in security service
		/// </summary>
		/// <param name="service">Security service</param>
		/// <param name="ioc">Ioc container, from where providers will be requested</param>
		public static void InitializeModuleSecurityProviders(this ISecurityService service, Container ioc)
		{
			if (service == null)
				throw new ArgumentNullException("service");
			if (ioc == null)
				throw new ArgumentNullException("ioc");

			foreach (var provider in ioc.GetAllInstances<ISecurityConstraintsProvider>())
			{
				service.RegisterProvider(provider);
			}
		}
	}
}