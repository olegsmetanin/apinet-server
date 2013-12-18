using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Reporting;
using AGO.Core.Modules.Attributes;
using Newtonsoft.Json;

namespace AGO.Core.Controllers
{
	public class ReportingController: AbstractController
	{
		private const string TEMPLATE_ID_FORM_KEY = "templateId";
		private string uploadPath;

		public ReportingController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ICrudDao crudDao, 
			IFilteringDao filteringDao, 
			ISessionProvider sessionProvider, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController) 
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
		}

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("UploadPath".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				uploadPath = value;
			else
				base.DoSetConfigProperty(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("UploadPath".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return uploadPath;
			return base.DoGetConfigProperty(key);
		}

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();
			if (uploadPath.IsNullOrWhiteSpace())
				uploadPath = System.IO.Path.GetTempPath();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ReportTemplateModel> GetTemplates(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			//TODO templates for system, module, project, project member

			return _FilteringDao.List<ReportTemplateModel>(filter, page, sorters);
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetTemplatesCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			//TODO templates for system, module, project, project member

			return _FilteringDao.RowCount<ReportTemplateModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public UploadedFiles UploadTemplate([NotEmpty]HttpRequestBase request, [NotEmpty]HttpFileCollectionBase files)
		{
			var result = new UploadResult[files.Count];
			for(var fileIndex = 0; fileIndex < files.Count; fileIndex++)
			{
				var idx = fileIndex;
				var file = files[idx];
				Debug.Assert(file != null);
				var sTemplateId = request.Form[TEMPLATE_ID_FORM_KEY];
				var templateId = !sTemplateId.IsNullOrWhiteSpace() ? new Guid(sTemplateId) : (Guid?) null;
				new Uploader(uploadPath).HandleRequest(request, file, 
					(fileName, buffer) =>
						{

							var checkQuery = _SessionProvider.CurrentSession.QueryOver<ReportTemplateModel>();
							checkQuery = templateId.HasValue
							             	? checkQuery.Where(m => m.Name == fileName && m.Id != templateId)
							             	: checkQuery.Where(m => m.Name == fileName);
							var count = checkQuery.ToRowCountQuery().UnderlyingCriteria.UniqueResult<int>();
							if (count > 0)
								throw new MustBeUniqueException();

							var template = templateId.HasValue 
								? _CrudDao.Get<ReportTemplateModel>(templateId)
								: new ReportTemplateModel { CreationTime = DateTime.UtcNow };
							template.Name = fileName;
							template.LastChange = DateTime.UtcNow;
							template.Content = buffer;

							_CrudDao.Store(template);

							result[idx] = new UploadResult
							{
								Name = template.Name,
								Length = template.Content.Length,
								Type = file.ContentType,
								Model = template
							};
						}
				);
			}

			return new UploadedFiles { Files = result };
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteTemplate([NotEmpty]Guid templateId)
		{
			var template = _CrudDao.Get<ReportTemplateModel>(templateId);
			//TODO security checks
			if (_CrudDao.Exists<ReportSettingModel>(q => q.Where(m => m.ReportTemplate == template)))
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(template);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> TemplateMetadata()
		{
			return MetadataForModelAndRelations<ReportTemplateModel>();
		}

		public class UploadResult
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("length")]
			public int Length { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("model")]
			public ReportTemplateModel Model { get; set; }
		}

		public class UploadedFiles
		{
			[JsonProperty("files")]
			public IEnumerable<UploadResult> Files { get; set; }
		}
	}
}