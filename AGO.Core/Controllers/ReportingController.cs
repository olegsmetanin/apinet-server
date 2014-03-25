using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
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
using AGO.Core.Security;
using AGO.Core.Watchers;
using AGO.Reporting.Common;
using AGO.Reporting.Common.Model;
using AGO.WorkQueue;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;

namespace AGO.Core.Controllers
{
	public class ReportingController: AbstractController
	{
		private const string PROJECT_FORM_KEY = "project";
		private const string TEMPLATE_ID_FORM_KEY = "templateId";
		private string uploadPath;
		private readonly INotificationService bus;
		private readonly IWorkQueue workQueue;

		public ReportingController(
			IJsonService jsonService, 
			IFilteringService filteringService, 
			ILocalizationService localizationService, 
			IModelProcessingService modelProcessingService, 
			AuthController authController,
			INotificationService notificationService,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory,
			IWorkQueue workQueue) 
			: base(jsonService, filteringService, localizationService, modelProcessingService, authController, securityService, registry, factory)
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

		private static IDictionary<string, LookupEntry[]> statesCache = new Dictionary<string, LookupEntry[]>();
		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupStates(string term, [InRange(0, null)] int page)
		{
			return LookupEnum<ReportTaskState>(term, page, ref statesCache);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ReportTemplateModel> GetTemplates(
			[NotEmpty] string project,
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			IModelFilterNode projectPredicate = _FilteringService.Filter<ReportTemplateModel>()
				.Where(m => m.ProjectCode == project);
			filter.Add(projectPredicate);

			return DaoFactory.CreateProjectFilteringDao(project).List<ReportTemplateModel>(filter, page, sorters);
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetTemplatesCount([NotNull] string project, [NotNull] ICollection<IModelFilterNode> filter)
		{
			IModelFilterNode projectPredicate = _FilteringService.Filter<ReportTemplateModel>()
				.Where(m => m.ProjectCode == project);
			filter.Add(projectPredicate);

			return DaoFactory.CreateProjectFilteringDao(project).RowCount<ReportTemplateModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public UploadedFiles<ReportTemplateModel> UploadTemplate([NotEmpty]HttpRequestBase request, [NotEmpty]HttpFileCollectionBase files)
		{
			var result = new UploadResult<ReportTemplateModel>[files.Count];
			for(var fileIndex = 0; fileIndex < files.Count; fileIndex++)
			{
				var idx = fileIndex;
				var file = files[idx];
				Debug.Assert(file != null);
				var sTemplateId = request.Form[TEMPLATE_ID_FORM_KEY];
				var templateId = !sTemplateId.IsNullOrWhiteSpace() ? new Guid(sTemplateId) : (Guid?) null;
				var project = request[PROJECT_FORM_KEY];
				if (project.IsNullOrWhiteSpace())
					throw new ArgumentException("Not found project code in upload request", "request");
				new Uploader(uploadPath).HandleRequest(request, file, 
					(fileName, buffer) =>
						{

							var checkQuery = ProjectSession(project).QueryOver<ReportTemplateModel>();
							checkQuery = templateId.HasValue
							             	? checkQuery.Where(m => m.ProjectCode == project && m.Name == fileName && m.Id != templateId)
											: checkQuery.Where(m => m.ProjectCode == project && m.Name == fileName);
							var count = checkQuery.ToRowCountQuery().UnderlyingCriteria.UniqueResult<int>();
							if (count > 0)
								throw new MustBeUniqueException();

							var dao = DaoFactory.CreateProjectCrudDao(project);
							var template = templateId.HasValue 
								? dao.Get<ReportTemplateModel>(templateId)
								: new ReportTemplateModel { ProjectCode = project, CreationTime = DateTime.UtcNow };
							template.Name = fileName;
							template.LastChange = DateTime.UtcNow;
							template.Content = buffer;

							SecurityService.DemandUpdate(template, template.ProjectCode, CurrentUser.Id, ProjectSession(project));

							dao.Store(template);

							result[idx] = new UploadResult<ReportTemplateModel>
							{
								Name = template.Name,
								Length = template.Content.Length,
								Type = file.ContentType,
								Model = template
							};
						}
				);
			}

			return new UploadedFiles<ReportTemplateModel> { Files = result };
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteTemplate([NotEmpty] string project, [NotEmpty]Guid templateId)
		{
			var dao = DaoFactory.CreateProjectCrudDao(project);
			var template = dao.Get<ReportTemplateModel>(templateId);
			
			SecurityService.DemandDelete(template, template.ProjectCode, CurrentUser.Id, ProjectSession(template.ProjectCode));

			if (dao.Exists<ReportSettingModel>(q => q.Where(m => m.ReportTemplate.Id == template.Id)))
				throw new CannotDeleteReferencedItemException();

			dao.Delete(template);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> TemplateMetadata()
		{
			return MetadataForModelAndRelations<ReportTemplateModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<ReportSettingModel> GetSettings([NotEmpty] string project, [NotEmpty] string[] types)
		{
			return ProjectSession(project).QueryOver<ReportSettingModel>()
				.Where(m => m.ProjectCode == project)
				.WhereRestrictionOn(m => m.TypeCode).IsIn(types.Cast<object>().ToArray())
				.OrderBy(m => m.Name).Asc
				.UnderlyingCriteria.List<ReportSettingModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupSettings([NotEmpty] string project, string term, [InRange(0, null)] int page)
		{
			var query = ProjectSession(project).QueryOver<ReportSettingModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.Name).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			return DaoFactory.CreateProjectCrudDao(project).PagedQuery(query, page).LookupModelsList(m => m.Name);
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
				var dao = DaoFactory.CreateProjectCrudDao(project);
				var settings = dao.Get<ReportSettingModel>(settingsId);
				var member = CurrentUserToMember(project);

				var name = (!resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : settings.Name)
				           + " " + DateTime.UtcNow.ToString("yyyy-MM-dd");
				var task = new ReportTaskModel
				           	{
				           		CreationTime = DateTime.UtcNow,
				           		Creator = member,
				           		State = ReportTaskState.NotStarted,
				           		ReportSetting = settings,
								ProjectCode = project,
								Name = name, 
								Parameters = parameters.ToStringSafe(),
								ResultName = !resultName.IsNullOrWhiteSpace() ? resultName.TrimSafe() : null,
								ResultUnread = true,
								Culture = CultureInfo.CurrentUICulture.Name
				           	};
				dao.Store(task);

				//Add task to system shared work queue, so one of workers can grab and execute it
				var qi = new QueueItem("Report", task.Id, project, CurrentUser.Id.ToString())
				{
					PriorityType = priority,
					UserPriority = member != null ? member.UserPriority : 0
				};
				workQueue.Add(qi);
				//emit event for reporting service (about new task to work)
				bus.EmitRunReport(task.Id);
				//emit event for client (about his task in queue and start soon)
				bus.EmitReportChanged(ReportEvents.CREATED, CurrentUser.Id.ToString(),  ReportTaskToDTO(task));
			}
			catch (Exception ex)
			{
				Log.Error("Ошибка при создании задачи на генерацию отчета", ex);
				throw;
			}
		}

		private ReportTaskModel PrepareForCancelReport(Guid taskId, ICrudDao dao)
		{
			var task = dao.Get<ReportTaskModel>(taskId);
			//emit event for reporting service (interrupt task if running)
			bus.EmitCancelReport(task.Id);

			if (task.State == ReportTaskState.NotStarted)
			{
				//task may be in work queue, if not started yet, remove
				workQueue.Remove(task.Id);
			}
			return task;
		}

		[JsonEndpoint, RequireAuthorization]
		public void CancelReport([NotEmpty] string project, [NotEmpty] Guid id)
		{
			var dao = DaoFactory.CreateProjectCrudDao(project);
			var task = PrepareForCancelReport(id, dao);

			task.State = ReportTaskState.Canceled;
			task.CompletedAt = DateTime.Now;
			task.ErrorMsg = "Canceled by user request";
			dao.Store(task);

			//emit event for client (about his task is canceled)
			var dto = ReportTaskToDTO(task);
			bus.EmitReportChanged(ReportEvents.CANCELED, CurrentUser.Id.ToString(), dto);
			if (task.Creator != null && CurrentUser.Id != task.Creator.Id)
			{
				//and to creator, if another person cancel task (admin, for example)
				bus.EmitReportChanged(ReportEvents.CANCELED, task.AuthorId.ToString(), dto);
			}

			ProjectSession(project).Flush();
		}


		[JsonEndpoint, RequireAuthorization]
		public void DeleteReport([NotEmpty] string project, [NotEmpty] Guid id)
		{
			var dao = DaoFactory.CreateProjectCrudDao(project);
			var task = PrepareForCancelReport(id, dao);
			
			var dto = ReportTaskToDTO(task);
			dao.Delete(task);

			//emit event for client (about his task is successfully deleted)
			bus.EmitReportChanged(ReportEvents.DELETED, CurrentUser.Id.ToString(), dto);
			if (task.Creator != null && CurrentUser.Id != task.Creator.Id)
			{
				//and to creator, if another person delete task (admin, for example)
				bus.EmitReportChanged(ReportEvents.DELETED, task.AuthorId.ToString(), dto);
			}

			ProjectSession(project).Flush();
		}

		private const int TOP_REPORTS = 10;

		[JsonEndpoint, RequireAuthorization]
		public object GetTopLastReports([NotEmpty] string project)
		{
			//TODO: Remove after FIXME
			return new
			{
				active = 0,
				unread = 0,
				reports = new object[0]
			};

			var user = _AuthController.CurrentUser();

			Func<ISession,
				Expression<Func<ReportTaskModel, bool>>,
				IQueryOver<ReportTaskModel, ReportTaskModel>> buildQuery = (s, predicate) =>
				{
					ReportTaskModel taskAlias = null;
					var q = s.QueryOver(() => taskAlias).Where(() => taskAlias.ProjectCode == project);
					if (predicate != null)
						q = q.Where(predicate);
					if (user.SystemRole != SystemRole.Administrator)
					{
						ProjectMemberModel memberAlias = null;
						q.JoinAlias(() => taskAlias.Creator, () => memberAlias);
						q = q.Where(() => memberAlias.UserId == user.Id);
					}
					return q;
				};

			var projSessionFactory = SessionProviderRegistry.GetProjectProvider(project).SessionFactory;
			Action<Action<ISession>> doInSessionContext = action =>
			{
				//separate session - don't use CurrentSession, that binded to request thread
				using (var session = projSessionFactory.OpenSession())
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
					reports = buildQuery(s, m => m.State == ReportTaskState.NotStarted || m.State == ReportTaskState.Running)
						.Fetch(m => m.Creator).Eager //beause after doInSessionContext session will be closed and then the user proxy will not be loaded
						.OrderBy(m => m.CreationTime).Desc
						.UnderlyingCriteria.SetMaxResults(TOP_REPORTS)
						.List<ReportTaskModel>();
					if (reports.Count < TOP_REPORTS)
					{
						reports = reports.Concat(
								buildQuery(s, m => m.State != ReportTaskState.NotStarted && m.State != ReportTaskState.Running)
									.Fetch(m => m.Creator).Eager
									.OrderBy(m => m.CreationTime).Desc
									.UnderlyingCriteria.SetMaxResults(TOP_REPORTS - reports.Count)
									.List<ReportTaskModel>())
								.ToList();
					}
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

		private void AddReportsPredicate(string project, ICollection<IModelFilterNode> filters)
		{
			var fb = _FilteringService.Filter<ReportTaskModel>();
			filters.Add(fb.Where(m => m.ProjectCode == project));
			if (CurrentUser.SystemRole != SystemRole.Administrator)
			{
				filters.Add(fb.Where(m => m.Creator.UserId == CurrentUser.Id));
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable GetReports(
			[NotEmpty] string project,
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			AddReportsPredicate(project, filter);
			return DaoFactory.CreateProjectFilteringDao(project).List<ReportTaskModel>(filter, page, sorters).Select(ReportTaskToDTO).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetReportsCount([NotEmpty] string project, [NotNull] ICollection<IModelFilterNode> filter)
		{
			AddReportsPredicate(project, filter);
			return DaoFactory.CreateProjectFilteringDao(project).RowCount<ReportTaskModel>(filter);
		}

		private void AddArchivedReportsPredicate(ICollection<IModelFilterNode> filters)
		{
			var fb = _FilteringService.Filter<ReportArchiveRecordModel>();
			if (CurrentUser.SystemRole != SystemRole.Administrator)
			{
				filters.Add(fb.Where(m => m.UserId == CurrentUser.Id));
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable GetArchivedReports(
			[InRange(0, null)] int page,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters)
		{
			AddArchivedReportsPredicate(filter);
			return DaoFactory.CreateMainFilteringDao().List<ReportArchiveRecordModel>(filter, page, sorters).ToList();
		}

		[JsonEndpoint, RequireAuthorization]
		public int GetArchivedReportsCount([NotNull] ICollection<IModelFilterNode> filter)
		{
			AddArchivedReportsPredicate(filter);
			return DaoFactory.CreateMainFilteringDao().RowCount<ReportArchiveRecordModel>(filter);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ReportTaskMetadata()
		{
			return MetadataForModelAndRelations<ReportTaskModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<IModelMetadata> ArchivedReportMetadata()
		{
			return MetadataForModelAndRelations<ReportArchiveRecordModel>();
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<WorkQueueWatchService.ReportQueuePosition> GetReportQueuePositions()
		{
			var login = _AuthController.CurrentUser().Id.ToString();
			var snapshot = workQueue.Snapshot();
			return WorkQueueWatchService.GetPositionsForUser(login, snapshot).ToArray();
		}

		private ReportTaskDTO ReportTaskToDTO(ReportTaskModel task)
		{
			var p = MainSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == task.ProjectCode).SingleOrDefault();
			var project = p != null ? p.Name : null;
			return ReportTaskDTO.FromTask(task, _LocalizationService, project, CurrentUser.SystemRole != SystemRole.Administrator);
		}
	}
}