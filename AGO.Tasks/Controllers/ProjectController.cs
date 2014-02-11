using System;
using System.Collections.Generic;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Tasks.Controllers.DTO;


namespace AGO.Tasks.Controllers
{
	public class ProjectController: AbstractTasksController
	{
		public ProjectController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ICrudDao crudDao, 
			IFilteringDao filteringDao, 
			ISessionProvider sessionProvider, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController) : base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		private static IDictionary<string, LookupEntry[]> projectStatuses;

		[JsonEndpoint, RequireAuthorization]
		public ProjectDTO GetProject([NotEmpty] string project)
		{
			var p = _CrudDao.Find<ProjectModel>(q => q.Where(m => m.ProjectCode == project));

			if (p == null)
				throw new NoSuchProjectException();

			var adapter = new ProjectAdapter(_LocalizationService, _AuthController.CurrentUser());
			return adapter.Fill(p);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupProjectStatuses(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<ProjectStatus>(term, page, ref projectStatuses);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<ProjectDTO> UpdateProject([NotEmpty] string project, [NotNull] PropChangeDTO data)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			var user = _AuthController.CurrentUser();
			return Edit<ProjectModel, ProjectDTO>(data.Id, project,
				(p, vr) =>
				{
					if (data.Prop.IsNullOrWhiteSpace())
					{
						vr.AddErrors("Property name required");
						return;
					}

					try
					{
						switch (data.Prop)
						{
							case "Name":
								p.Name = data.Value.ConvertSafe<string>().TrimSafe();
								break;
							case "Description":
								p.Description = data.Value.ConvertSafe<string>().TrimSafe();
								break;
							case "VisibleForAll":
								if (!p.IsAdmin(user))
									throw new AccessForbiddenException();

								p.VisibleForAll = data.Value.ConvertSafe<bool>();
								break;
							case "Status":
								//TODO business logic, security and other checks
								var newStatus = data.Value.ConvertSafe<ProjectStatus>();
								p.ChangeStatus(newStatus, _AuthController.CurrentUser());//create entity saved via cascade
								break;
							default:
								vr.AddErrors(string.Format("Unsupported prop for update: '{0}'", data.Prop));
								break;
						}
					}
					catch (InvalidCastException cex)
					{
						vr.AddFieldErrors(data.Prop, cex.GetBaseException().Message);
					}
					catch (OverflowException oex)
					{
						vr.AddFieldErrors(data.Prop, oex.GetBaseException().Message);
					}

				},
				p => new ProjectAdapter(_LocalizationService, user).Fill(p),
				() => { throw new ProjectCreationNotSupportedException(); });
		}

		[JsonEndpoint, RequireAuthorization]
		public bool TagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = _SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).SingleOrDefault();

			if (projectToTag != null)
				return false;

			var project = _CrudDao.Get<ProjectModel>(modelId, true);
			if ((project.Creator == null || !currentUser.Equals(project.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			var tag = _CrudDao.Get<ProjectTagModel>(tagId, true);
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			_CrudDao.Store(new ProjectToTagModel
			{
				Creator = currentUser,
				Project = project,
				Tag = tag
			});

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DetagProject(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var currentUser = _AuthController.CurrentUser();

			var projectToTag = _SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
				.Where(m => m.Project.Id == modelId && m.Tag.Id == tagId).SingleOrDefault();

			if (projectToTag == null)
				return false;

			var project = projectToTag.Project;
			if ((project.Creator == null || !currentUser.Equals(project.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			var tag = projectToTag.Tag;
			if ((tag.Creator == null || !currentUser.Equals(tag.Creator)) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			_CrudDao.Delete(projectToTag);

			return true;
		}
	}
}
