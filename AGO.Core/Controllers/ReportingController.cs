using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Reporting;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Core.Notification;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.WorkQueue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate;

namespace AGO.Core.Controllers
{
	public class ReportingController: AbstractController
	{
		private const string TEMPLATE_ID_FORM_KEY = "templateId";
		private string uploadPath;
		private readonly INotificationService bus;
		private readonly IWorkQueue workQueue;

		public ReportingController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ICrudDao crudDao, 
			IFilteringDao filteringDao, 
			ISessionProvider sessionProvider, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController,
			INotificationService notificationService,
			IWorkQueue workQueue) 
			: base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController)
		{
			if (notificationService == null)
				throw new ArgumentNullException("notificationService");
			if (workQueue == null)
				throw new ArgumentNullException("workQueue");

			bus = notificationService;
			this.workQueue = workQueue;
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
			if (_CrudDao.Exists<ReportSettingModel>(q => q.Where(m => m.ReportTemplate.Id == template.Id)))
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
		public void RunReport(
			[NotEmpty] string project,
			[NotEmpty] Guid settingsId, 
			int priority,
			string resultName, 
			JObject parameters)
		{
			try
			{
				var settings = _CrudDao.Get<ReportSettingModel>(settingsId);
				var user = _AuthController.CurrentUser();
				var participant = _FilteringService.Filter<ProjectParticipantModel>()
					.Where(m => m.User.Id == user.Id && m.Project.ProjectCode == project)
					.List(_FilteringDao).FirstOrDefault();

				var name = (!resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : settings.Name)
				           + " " + DateTime.UtcNow.ToString("yyyy-MM-dd");
				var task = new ReportTaskModel
				           	{
				           		CreationTime = DateTime.UtcNow,
				           		Creator = user,
				           		State = ReportTaskState.NotStarted,
				           		ReportSetting = settings,
								Project = project,
								Name = name, 
								Parameters = parameters.ToStringSafe(),
								ResultName = !resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : null,
								ResultUnread = true
				           	};
				_CrudDao.Store(task);
				_SessionProvider.FlushCurrentSession();

				//Add task to system shared work queue, so one of workers can grab and execute it
				var qi = new QueueItem("Report", task.Id, project, user.Login)
				{
					PriorityType = priority,
					UserPriority = participant != null ? participant.UserPriority : 0
				};
				workQueue.Add(qi);
				//emit event for reporting service (about new task to work)
				bus.EmitRunReport(task.Id);
				//emit event for client (about his task in queue and start soon)
				bus.EmitReportChanged(ReportEvents.CREATED, user.Login,  ReportTaskToDTO(task));
			}
			catch (Exception ex)
			{
				Log.Error("Ошибка при создании задачи на генерацию отчета", ex);
				throw;
			}
		}

		private const int TOP_REPORTS = 10;

		[JsonEndpoint, RequireAuthorization]
		public object GetTopLastReports()
		{
			var user = _AuthController.CurrentUser();

			Func<ISession, 
				Expression<Func<ReportTaskModel, bool>>, 
				IQueryOver<ReportTaskModel, ReportTaskModel>> buildQuery = (s, predicate) =>
			{
				var q = s.QueryOver<ReportTaskModel>();
				if (predicate != null)
					q = q.Where(predicate);
				if (user.SystemRole != SystemRole.Administrator)
					q = q.Where(m => m.Creator.Id == user.Id);
				return q;
			};

			Action<Action<ISession>> doInSessionContext = action =>
			{
				//separate session - don't use CurrentSession, that binded to request thread
				using (var session = _SessionProvider.SessionFactory.OpenSession())
				{
					action(session);
					session.Close();
				}
			};

			var activeCountTask = Task.Factory.StartNew(() =>
			{
				var count = 0;
				doInSessionContext(s =>
				{
					count = buildQuery(s, m => m.State == ReportTaskState.Running || m.State == ReportTaskState.NotStarted).RowCount();
				});
				return count;
			});
			var unreadCountTask = Task.Factory.StartNew(() =>
			{
				var count = 0;
				doInSessionContext(s =>
				{
					count = buildQuery(s, m => m.State == ReportTaskState.Completed && m.ResultUnread).RowCount();
				});
				return count;
			});
			var topLastTask = Task.Factory.StartNew(() =>
			{
				IList<ReportTaskModel> reports = null;
				doInSessionContext(s =>
				{
					reports = buildQuery(s, null)
						.Fetch(m => m.Creator).Eager //beause after doInSessionContext session will be closed and then the user proxy will not be loaded
						.OrderBy(m => m.CreationTime).Desc
						.UnderlyingCriteria.SetMaxResults(TOP_REPORTS)
						.List<ReportTaskModel>();
				});
				return reports;
			});

			Task.WaitAll(activeCountTask, unreadCountTask, topLastTask);

			return new
			{
				active = activeCountTask.Result,
				unread = unreadCountTask.Result,
				reports = topLastTask.Result.Select(ReportTaskToDTO).ToList()
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public void CancelReport([NotEmpty] Guid id)
		{
			var task = _CrudDao.Get<ReportTaskModel>(id);
			//emit event for reporting service (interrupt task if running or waiting)
			bus.EmitCancelReport(task.Id);

			task.State = ReportTaskState.Canceled;
			task.CompletedAt = DateTime.Now;
			task.ErrorMsg = "Canceled by user request";
			_CrudDao.Store(task);

			//emit event for client (about his task is canceled)
			var dto = ReportTaskToDTO(task);
			bus.EmitReportChanged(ReportEvents.CANCELED, _AuthController.CurrentUser().Login, dto);
			if (task.Creator != null && !_AuthController.CurrentUser().Equals(task.Creator))
			{
				//and to creator, if another person cancel task (admin, for example)
				bus.EmitReportChanged(ReportEvents.CANCELED, task.AuthorLogin, dto);
			}

			_SessionProvider.FlushCurrentSession();
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteReport([NotEmpty] Guid id)
		{
			var task = _CrudDao.Get<ReportTaskModel>(id);
			//May be this task is running
			bus.EmitCancelReport(task.Id);

			var dto = ReportTaskToDTO(task);
			_CrudDao.Delete(task);

			//emit event for client (about his task is successfully deleted)
			bus.EmitReportChanged(ReportEvents.DELETED, _AuthController.CurrentUser().Login, dto);
			if (task.Creator != null && !_AuthController.CurrentUser().Equals(task.Creator))
			{
				//and to creator, if another person delete task (admin, for example)
				bus.EmitReportChanged(ReportEvents.DELETED, task.AuthorLogin, dto);
			}

			_SessionProvider.FlushCurrentSession();
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
				filter.Add(_FilteringService.Filter<ReportTaskModel>().Where(m => m.Creator.Id == user.Id));
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
				filter.Add(_FilteringService.Filter<ReportTaskModel>().Where(m => m.Creator.Id == user.Id));
			}

			return _FilteringDao.RowCount<ReportTaskModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ReportTaskMetadata()
		{
			return MetadataForModelAndRelations<ReportTaskModel>();
		}

		private ReportTaskDTO ReportTaskToDTO(ReportTaskModel task)
		{
			var p = _SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == task.Project).SingleOrDefault();
			var project = p != null ? p.Name : null;
			return ReportTaskDTO.FromTask(task, _LocalizationService, project, _AuthController.CurrentUser().SystemRole != SystemRole.Administrator);
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