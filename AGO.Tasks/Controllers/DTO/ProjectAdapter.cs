using System;
using System.Linq;
using AGO.Core.Controllers;
using AGO.Core.Localization;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Адаптер модели проекта
	/// </summary>
	public sealed class ProjectAdapter: ModelAdapter<ProjectModel, ProjectDTO>
	{
		private readonly ILocalizationService localization;
		private readonly UserModel user;

		public ProjectAdapter(ILocalizationService localizationService, UserModel currentUser)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			if (currentUser == null)
				throw new ArgumentNullException("currentUser");

			localization = localizationService;
			user = currentUser;
		}

		private LookupEntry ProjectStatusToLookup(ProjectStatus status)
		{
			return new LookupEntry
			{
				Id = status.ToString(),
				Text = localization.MessageForType(typeof(ProjectStatus), status) ?? status.ToString()
			};
		}

		public override ProjectDTO Fill(ProjectModel model)
		{
			var dto = base.Fill(model);
			dto.Author = ToAuthor(model);
			dto.CreationTime = model.CreationTime;
			dto.Name = model.Name;

			dto.ProjectCode = model.ProjectCode;
			dto.Type = model.Type.Name;
			dto.Status = ProjectStatusToLookup(model.Status);
			dto.Description = model.Description;
			dto.VisibleForAll = model.VisibleForAll;

			dto.Tags = (from ptt in model.Tags
				where ptt.Tag.CreatorId != null && ptt.Tag.CreatorId == user.Id
				orderby ptt.Tag.CreatorId, ptt.Tag.FullName
				select new LookupEntry { Id = ptt.Tag.Id.ToString(), Text = ptt.Tag.FullName })
				.ToArray();

			return dto;
		}
	}
}