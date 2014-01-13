using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Notifications;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		public IEnumerable<LookupEntry> GetServices()
		{
			//TODO calculate available services for user and/or project/module/...
			return _SessionProvider.CurrentSession
				.QueryOver<ReportingServiceDescriptorModel>()
				.LookupModelsList(m => m.Name);
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

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ReportSettingModel> GetSettings([NotEmpty] string[] types)
		{
			return _SessionProvider.CurrentSession.QueryOver<ReportSettingModel>()
				.WhereRestrictionOn(m => m.TypeCode).IsIn(types.Cast<object>().ToArray())
				.OrderBy(m => m.Name).Asc
				.UnderlyingCriteria.List<ReportSettingModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public ReportTaskModel RunReport([NotEmpty] Guid serviceId, [NotEmpty] Guid settingsId, string resultName, JObject parameters)
		{
			try
			{
				var service = _CrudDao.Get<ReportingServiceDescriptorModel>(serviceId);
				var settings = _CrudDao.Get<ReportSettingModel>(settingsId);

				var name = (!resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : settings.Name)
				           + " " + DateTime.UtcNow.ToString("yyyy-MM-dd");
				var task = new ReportTaskModel
				           	{
				           		CreationTime = DateTime.UtcNow,
				           		Creator = _AuthController.CurrentUser(),
				           		State = ReportTaskState.NotStarted,
				           		ReportSetting = settings,
				           		ReportingService = service,
								Name = name, 
								Parameters = parameters.ToStringSafe(),
								ResultName = !resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : null,
								ResultUnread = true
				           	};
				_CrudDao.Store(task);
				_SessionProvider.FlushCurrentSession();

				//Replace with call to redis publish
				//var hub = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
				//hub.Clients.All.onReportChanged(task.Id);

				using (var client = new ServiceClient(service.EndPoint))
				{
					client.RunReport(task.Id);
					//TODO: replace with call to redis publishing
					//hub.Clients.All.onReportChanged(task.Id);
				}

				return task;
			}
			catch (Exception ex)
			{
				Log.Error("Ошибка при создании задачи на генерацию отчета", ex);
				throw;
			}
		}

		private const int TOP_REPORTS = 10;

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable GetTopLastReports()
		{
			var running = _SessionProvider.CurrentSession.QueryOver<ReportTaskModel>()
				.Where(m => m.State == ReportTaskState.Running || m.State == ReportTaskState.NotStarted)
				.OrderBy(m => m.CreationTime).Desc
				.UnderlyingCriteria.SetMaxResults(10)
				.List<ReportTaskModel>()
				.Select(ReportTaskToDTO)
				.ToList();

			var unread = Enumerable.Empty<object>();
			var rest = TOP_REPORTS - running.Count;
			if (rest > 0)
			{
				unread = _SessionProvider.CurrentSession.QueryOver<ReportTaskModel>()
					.Where(m => m.State != ReportTaskState.NotStarted && m.State != ReportTaskState.Running)
					.OrderBy(m => m.CreationTime).Desc
					.UnderlyingCriteria.SetMaxResults(rest)
					.List<ReportTaskModel>()
					.Select(ReportTaskToDTO)
					.ToList();
			}

			return running.Concat(unread);
		}

		[JsonEndpoint, RequireAuthorization]
		public object CancelReport([NotEmpty] Guid id)
		{
			var task = _CrudDao.Get<ReportTaskModel>(id);
			using (var client = new ServiceClient(task.Service.EndPoint))
			{
				if (!client.CancelReport(task.Id))
				{
					_SessionProvider.CurrentSession.Refresh(task);
					if (task.State == ReportTaskState.NotStarted || task.State == ReportTaskState.Running)
					{
						task.State = ReportTaskState.Canceled;
						_CrudDao.Store(task);
						_SessionProvider.FlushCurrentSession();
					}
				}
				_SessionProvider.CurrentSession.Refresh(task);
				return ReportTaskToDTO(task);
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteReport([NotEmpty] Guid id)
		{
			var task = _CrudDao.Get<ReportTaskModel>(id);
			using (var client = new ServiceClient(task.Service.EndPoint))
			{
				try
				{
					if (client.IsRunning(task.Id) || client.IsWaitingForRun(task.Id))
						client.CancelReport(task.Id);
				}
				catch(Exception ex)
				{
					Log.Error("Error when attempt to cancel report", ex);
				}
				finally
				{
					//delete in any situation
					try
					{
						_SessionProvider.CurrentSession.Refresh(task);
						_CrudDao.Delete(task);
					}
					catch (DataAccessException)
					{
						//may be we try to delete record, that was updated by reporting service,
						//retry one time
						_SessionProvider.CurrentSession.Refresh(task);
						_CrudDao.Delete(task);
					}
				}
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable GetReports(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			//TODO templates for system, module, project, project member

			var user = _AuthController.CurrentUser();
			if (user.SystemRole != SystemRole.Administrator)
			{
				filter.Add(_FilteringService.Filter<ReportTaskModel>().Where(m => m.Creator == user));
			}

			return _FilteringDao.List<ReportTaskModel>(filter, page, sorters).Select(ReportTaskToDTO).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetReportsCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			//TODO templates for system, module, project, project member
			var user = _AuthController.CurrentUser();
			if (user.SystemRole != SystemRole.Administrator)
			{
				filter.Add(_FilteringService.Filter<ReportTaskModel>().Where(m => m.Creator == user));
			}

			return _FilteringDao.RowCount<ReportTaskModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ReportTaskMetadata()
		{
			return MetadataForModelAndRelations<ReportTaskModel>();
		}

		private object ReportTaskToDTO(ReportTaskModel task)
		{
			return new
			{
				task.Id, task.Name, task.State,
				StateName = _LocalizationService.MessageForType(typeof(ReportTaskState), task.State) ?? task.State.ToString(),
				Author = task.Creator.FullName,
				task.CreationTime, task.StartedAt, task.CompletedAt,
				task.DataGenerationProgress, task.ReportGenerationProgress,
				task.ErrorMsg, 
				ErrorDetails = _AuthController.CurrentUser().SystemRole == SystemRole.Administrator ? task.ErrorDetails : null,
				task.ResultUnread
			};
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