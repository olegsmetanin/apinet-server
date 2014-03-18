using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Controllers.Security;
using AGO.Core.DataAccess;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Modules.Attributes;
using AGO.Core.Security;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;

namespace AGO.Tasks.Controllers
{
    /// <summary>
    /// Контроллер работы с задачами модуля задач
    /// </summary>
    public class TasksController: AbstractTasksController, IFileResourceStorage
	{
		#region Configuration

		private string uploadPath;
	    private string fileStoreRoot;

        public TasksController(
            IJsonService jsonService, 
            IFilteringService filteringService,
            ICrudDao crudDao, 
            IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			ILocalizationService localizationService,
			IModelProcessingService modelProcessingService,
			AuthController authController,
			ISecurityService securityService,
			ISessionProviderRegistry registry,
			DaoFactory factory)
            : base(jsonService, filteringService, crudDao, filteringDao, sessionProvider, localizationService, modelProcessingService, authController, securityService, registry, factory)
        {	
        }

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("UploadPath".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				uploadPath = value;
			else if ("FileStoreRoot".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				fileStoreRoot = value;
			else
				base.DoSetConfigProperty(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("UploadPath".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return uploadPath;
			if ("FileStoreRoot".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return fileStoreRoot;
			return base.DoGetConfigProperty(key);
		}

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();
			if (uploadPath.IsNullOrWhiteSpace())
				uploadPath = Path.GetTempPath();
			if (fileStoreRoot.IsNullOrWhiteSpace())
				Log.Warn("FileStoreRoot path not found in config (Tasks_Tasks_FileStoreRoot). Attempt to upload file to task will be failed");
		}

		#endregion

		#region Task

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<LookupEntry> LookupTasks(
			[NotEmpty] string project,
			string term,
			[InRange(0, null)] int page)
		{
			return Lookup<TaskModel>(project, term, page, m => m.SeqNumber, null, m => m.InternalSeqNumber, m => m.SeqNumber);
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<TaskListItemDTO> GetTasks(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			[InRange(0, null)] int page,
			TaskPredefinedFilter predefined)
		{
			var predicate = MakeTasksPredicate(project, filter, predefined);
			var adapter = new TaskListItemAdapter(_LocalizationService, _AuthController.CurrentUser());

			return _FilteringDao.List<TaskModel>(predicate, page, sorters)
				.Select(adapter.Fill)
				.ToArray();
		}

    	[JsonEndpoint, RequireAuthorization]
		public int GetTasksCount(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			TaskPredefinedFilter predefined)
    	{
    		var predicate = MakeTasksPredicate(project, filter, predefined);
    		return _FilteringDao.RowCount<TaskModel>(predicate);
    	}

		private IModelFilterNode MakeTasksPredicate(string project, IEnumerable<IModelFilterNode> filter, TaskPredefinedFilter predefined)
		{
			var projectPredicate = _FilteringService.Filter<TaskModel>().Where(m => m.ProjectCode == project);
			var predefinedPredicate = predefined.ToFilter(_FilteringService.Filter<TaskModel>());
			var predicate = SecurityService.ApplyReadConstraint<TaskModel>(project, CurrentUser.Id, Session, 
				filter.Concat(new[] { projectPredicate, predefinedPredicate }).ToArray());
			return predicate;
		}

	    private TaskModel FindTaskByProjectAndNumber(string project, string numpp)
	    {
			var fb = _FilteringService.Filter<TaskModel>();
			var predicate = SecurityService.ApplyReadConstraint<TaskModel>(project, CurrentUser.Id, Session,
				fb.Where(m => m.ProjectCode == project && m.SeqNumber == numpp));

			var task = _FilteringDao.Find<TaskModel>(predicate);

			if (task == null)
				throw new NoSuchEntityException();

		    return task;
	    }

		[JsonEndpoint, RequireAuthorization]
		public TaskListItemDetailsDTO GetTaskDetails([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = FindTaskByProjectAndNumber(project, numpp);
			var adapter = new TaskListItemDetailsAdapter(_LocalizationService);
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskViewDTO GetTask([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = FindTaskByProjectAndNumber(project, numpp);			
			var adapter = new TaskViewAdapter(_LocalizationService);
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<string> CreateTask([NotEmpty] string project, [NotNull] CreateTaskDTO model)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			return Edit<TaskModel, string>(default(Guid), project, (task, vr) =>
				{
					if (model.TaskType == default(Guid))
						vr.AddFieldErrors("TaskType", _LocalizationService.MessageForException(new RequiredValueException()));
					if (model.Executors == null || model.Executors.Length <= 0)
						vr.AddFieldErrors("Executors", _LocalizationService.MessageForException(new RequiredValueException()));
					if (!vr.Success)
						return;

			        //FIXME: это надо переделать на sequence или свой аналог
					var num = Session.QueryOver<TaskModel>()
						.Where(m => m.ProjectCode == project)
						.UnderlyingCriteria
						.SetProjection(Projections.Max<TaskModel>(m => m.InternalSeqNumber))
						.UniqueResult<long?>().GetValueOrDefault(0) + 1;

			        task.InternalSeqNumber = num;
			        task.SeqNumber = "t0-" + num;
			        task.TaskType = _CrudDao.Get<TaskTypeModel>(model.TaskType);
			        task.DueDate = model.DueDate;
			        task.Content = model.Content;
			        task.Priority = model.Priority;

			        task.ChangeStatus(TaskStatus.New, task.Creator);

			        foreach (var id in model.Executors ?? Enumerable.Empty<Guid>())
			        {
			            var member = _CrudDao.Get<ProjectMemberModel>(id);
			            if (member == null)
			            {
			                vr.AddFieldErrors("Executors", _LocalizationService.MessageForException(new NoSuchProjectMemberException()));
			                continue;
			            }
			            if (member.ProjectCode != task.ProjectCode)
			            {
							vr.AddFieldErrors("Executors", _LocalizationService.MessageForException(new NoSuchProjectMemberException()));
			                continue;
			            }

			            var executor = new TaskExecutorModel
			                       		{
			                       			Creator = task.Creator,
			                       			Task = task,
			                       			Executor = member
			                       		};
			            task.Executors.Add(executor);
			        }
				}, task => task.SeqNumber);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<TaskViewDTO> UpdateTask([NotEmpty] string project, [NotNull] PropChangeDTO data)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			return Edit<TaskModel, TaskViewDTO>(data.Id, project, (task, vr) =>
			{
				if (data.Prop.IsNullOrWhiteSpace())
				{
					throw new RequiredValueException();
				}

			    try
			    {
			    	switch (data.Prop)
			    	{
			    		case "Content":
			    			task.Content = (string) data.Value;
			    			break;
			    		case "Note":
			    			task.Note = (string) data.Value;
			    			break;
			    		case "Priority":
			    			task.Priority = (TaskPriority) Enum.Parse(typeof (TaskPriority), (string) data.Value);
			    			break;
			    		case "Status":
			    			var newStatus = (TaskStatus) Enum.Parse(typeof (TaskStatus), (string) data.Value);
			    			task.ChangeStatus(newStatus, CurrentUserToMember(project));
			    			break;
			    		case "TaskType":
			    			task.TaskType = _CrudDao.Get<TaskTypeModel>(data.Value.ConvertSafe<Guid>(), true);
			    			break;
			    		case "DueDate":
			    			var dd = data.Value.ConvertSafe<DateTime?>();
			    			if (dd != null && dd.Value < DateTime.Today.ToUniversalTime())
			    			{
			    				throw new DueDateBeforeTodayException();
			    			}
			    			task.DueDate = dd;
			    			break;
			    		case "Executors":
			    			var ids = (data.Value.ConvertSafe<JArray>() ?? new JArray())
			    				.Select(id => id.ConvertSafe<Guid>()).ToArray();
			    			var toRemove = task.Executors.Where(e => !ids.Contains(e.Executor.Id)).ToArray();
			    			var toAdd = ids.Where(id => task.Executors.All(e => !e.Executor.Id.Equals(id)))
			    				.Select(id => _CrudDao.Get<ProjectMemberModel>(id, true));

			    			foreach (var removed in toRemove)
			    			{
			    				task.Executors.Remove(removed);
			    				_CrudDao.Delete(removed);
			    			}
			    			foreach (var added in toAdd)
			    			{
			    				var executor = new TaskExecutorModel
			    				{
			    					Creator = CurrentUserToMember(project),
			    					Task = task,
			    					Executor = added
			    				};
			    				task.Executors.Add(executor);
			    				_CrudDao.Store(executor);
			    			}
			    			break;
			    		case "EstimatedTime":
			    			var time = data.Value.ConvertSafe<decimal?>(CultureInfo.CurrentUICulture);
			    			if (data.Value != null && time == null)
			    				time = data.Value.ConvertSafe<decimal?>(CultureInfo.InvariantCulture);
			    			if (data.Value != null && time == null)
			    			{
								throw new IncorrectEstimatedTimeValueException(data.Value);
			    			}
			    			task.EstimatedTime = time;
			    			break;
			    		default:
			    			throw new UnsupportedPropertyForUpdateException(data.Prop);
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
			    catch (TasksException ex)
			    {
					vr.AddFieldErrors(data.Prop, ex.GetBaseException().Message);
			    }
			}, 
			task => new TaskViewAdapter(_LocalizationService).Fill(task),
			() => { throw new TaskCreationNotSupportedException(); });
		}

		private void InternalDeleteTask(string project, Guid id)
		{
			var task = SecureFind<TaskModel>(project, id);

			DemandDelete(task, task.ProjectCode);

			_CrudDao.Delete(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTask([NotEmpty] string project, [NotEmpty] Guid id)
		{
			InternalDeleteTask(project, id);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTasks([NotEmpty] string project, [NotNull] ICollection<Guid> ids)
		{
			var s = _SessionProvider.CurrentSession;
			var trn = s.BeginTransaction();
			try
			{
				foreach (var id in ids)
					InternalDeleteTask(project, id);

				trn.Commit();
			}
			catch (Exception)
			{
				trn.Rollback();
				throw;
			}
			return true;
		}

		[JsonEndpointAttribute, RequireAuthorizationAttribute]
		public IEnumerable<IModelMetadata> TaskMetadata([NotEmpty] string project)
		{
			return MetadataForModelAndRelations<TaskModel>(project);
		}

		#endregion

		#region Agreements

		[JsonEndpoint, RequireAuthorization]
		public Agreement AddAgreemer([NotEmpty] Guid taskId, [NotEmpty] Guid participantId, DateTime? dueDate = null)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);
			var member = _CrudDao.Get<ProjectMemberModel>(participantId, true);

			if (task.Status == TaskStatus.Closed)
				throw new CanNotAddAgreemerToClosedTaskException();

			if (task.IsAgreemer(member))
				throw new AgreemerAlreadyAssignedToTaskException(member.FullName, task.SeqNumber);

			var agreement = new TaskAgreementModel
			{
				Creator = CurrentUserToMember(task.ProjectCode),
				Task = task,
				Agreemer = member,
				DueDate = dueDate
			};
			SecurityService.DemandUpdate(agreement, task.ProjectCode, CurrentUser.Id, Session);
			task.Agreements.Add(agreement);

			_CrudDao.Store(agreement);
			_CrudDao.Store(task);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool RemoveAgreement([NotEmpty] Guid taskId, [NotEmpty] Guid agreementId)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);
			
			var agreement = task.Agreements.FirstOrDefault(a => a.Id == agreementId);
			if (agreement == null) return false;
			SecurityService.DemandDelete(agreement, task.ProjectCode, CurrentUser.Id, Session);
			
			if (task.Status == TaskStatus.Closed)
				throw new CanNotRemoveAgreemerFromClosedTaskException();
					
			task.Agreements.Remove(agreement);

			_CrudDao.Store(task);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public Agreement AgreeTask([NotEmpty] Guid taskId, string comment)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);
			
			var agreement = task.Agreements.FirstOrDefault(a => a.Agreemer.UserId == CurrentUser.Id);
			if (agreement == null)
				throw new CurrentUserIsNotAgreemerInTaskException();
			SecurityService.DemandUpdate(agreement, task.ProjectCode, CurrentUser.Id, Session);
			
			if (task.Status == TaskStatus.Closed)
				throw new CanNotAgreeClosedTaskException();

			agreement.Done = true;
			agreement.AgreedAt = DateTime.UtcNow;
			agreement.Comment = comment;

			_CrudDao.Store(agreement);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		[JsonEndpoint, RequireAuthorization]
		public Agreement RevokeAgreement([NotEmpty] Guid taskId)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);

			var agreement = task.Agreements.FirstOrDefault(a => a.Agreemer.UserId == CurrentUser.Id);
			if (agreement == null)
				throw new CurrentUserIsNotAgreemerInTaskException();
			SecurityService.DemandUpdate(agreement, task.ProjectCode, CurrentUser.Id, Session);

			if (task.Status == TaskStatus.Closed)
				throw new CanNotRevokeAgreementFromClosedTaskException();

			agreement.Done = false;
			agreement.AgreedAt = null;
			agreement.Comment = null;

			_CrudDao.Store(agreement);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		#endregion

		#region Tags

		[JsonEndpoint, RequireAuthorization]
		public bool TagTask(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var task = _CrudDao.Get<TaskModel>(modelId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);

			var link = task.Tags.FirstOrDefault(l => l.Tag.Id == tagId);

			if (link != null)
				return false;

			var tag = _CrudDao.Get<TaskTagModel>(tagId, true);
			link = new TaskToTagModel
			{
				Task = task,
				Tag = tag
			};
			SecurityService.DemandUpdate(link, task.ProjectCode, CurrentUser.Id, Session);
			task.Tags.Add(link);
			_CrudDao.Store(link);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DetagTask(
			[NotEmpty] Guid modelId,
			[NotEmpty] Guid tagId)
		{
			var task = _CrudDao.Get<TaskModel>(modelId, true);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);

			var link = task.Tags.FirstOrDefault(l => l.Tag.Id == tagId);
			if (link == null)
				return false;

			SecurityService.DemandDelete(link, task.ProjectCode, CurrentUser.Id, Session);
			task.Tags.Remove(link);
			_CrudDao.Delete(link);

			return true;
		}

		#endregion

		#region Params (user props)

		[JsonEndpoint, RequireAuthorization]
    	public IEnumerable<CustomParameterTypeDTO> LookupParamTypes(
			[NotEmpty] string project, 
			string term, 
			[InRange(0, null)] int page)
		{
			try
			{
				return PrepareLookup<CustomPropertyTypeModel>(project, term, page, m => m.FullName)
					.List<CustomPropertyTypeModel>().Select(TaskViewAdapter.ParamTypeToDTO);
			}
			catch (NoSuchProjectMemberException)
			{
				//same as in lookup method from base class
				Log.WarnFormat("Lookup from not project member catched. User '{0}' for type '{1}'", 
					CurrentUser.Email, typeof(CustomPropertyTypeModel).AssemblyQualifiedName);
				return Enumerable.Empty<CustomParameterTypeDTO>();
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<CustomParameterDTO> EditParam([NotEmpty] Guid taskId, [NotNull] CustomParameterDTO model)
		{
			var task = _CrudDao.Get<TaskModel>(taskId);
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);

			return Edit(model.Id, task.ProjectCode,
				(param, vr) => { param.Value = model.Value; },
				TaskViewAdapter.ParamToDTO,
				() => new TaskCustomPropertyModel
				{
					Creator = CurrentUserToMember(task.ProjectCode),
					Task = task,
					PropertyType = _CrudDao.Get<CustomPropertyTypeModel>(model.Type.Id, true)
				});
		}
			
		[JsonEndpoint, RequireAuthorization]
		public bool DeleteParam([NotEmpty] Guid paramId)
		{
			var param = _CrudDao.Get<TaskCustomPropertyModel>(paramId, true);
			if (param == null)
				throw new NoSuchEntityException();

			var task = param.Task;
			SecurityService.DemandUpdate(task, task.ProjectCode, CurrentUser.Id, Session);

			task.CustomProperties.Remove(param);
			_CrudDao.Delete(param);
			return true;
		}

		#endregion

		#region Files

	    private const string TASK_ID_FORM_KEY = "ownerId";
		private const string FILE_ID_FORM_KEY = "fileId";
	    private const string PROJECT_FORM_KEY = "project";

	    [JsonEndpoint, RequireAuthorization]
	    public UploadedFiles<FileDTO> UploadFiles(
		    [NotNull] HttpRequestBase request,
		    [NotEmpty] HttpFileCollectionBase files)
	    {
			var project = request.Form[PROJECT_FORM_KEY];
			if (project.IsNullOrWhiteSpace())
				throw new ArgumentException("Project not specified for request");
			var numpp = request.Form[TASK_ID_FORM_KEY];
			if (numpp.IsNullOrWhiteSpace())
				throw new ArgumentException("File owner id not specified for request");

		    var fb = _FilteringService.Filter<TaskModel>();
		    IModelFilterNode predicate = fb.Where(m => m.ProjectCode == project && m.SeqNumber == numpp);
			var securedPredicate = SecurityService.ApplyReadConstraint<TaskModel>(
			    project, CurrentUser.Id, Session, predicate);

		    var task = _FilteringDao.Find<TaskModel>(securedPredicate);
			if (task == null)
				throw new NoSuchEntityException();
			
			SecurityService.DemandUpdate(task, project, CurrentUser.Id, Session);

		    var result = new UploadResult<FileDTO>[files.Count];
		    var adapter = new FileAdapter();
		    for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
		    {
			    var idx = fileIndex;
			    var file = files[idx];
				Debug.Assert(file != null);
				new Uploader(uploadPath).HandleRequest(request, file, (fileName, content) =>
				{
					var fileId = !string.IsNullOrEmpty(request.Form[FILE_ID_FORM_KEY])
						? new Guid(request.Form[FILE_ID_FORM_KEY])
						: (Guid?)null;
					var taskFile = fileId.HasValue ? task.Files.FirstOrDefault(f => f.Id == fileId.Value) : null;
					//Test for unique file name
					if (task.Files.Any(f => f.Name == fileName && (taskFile == null || f.Id != taskFile.Id)))
						throw new MustBeUniqueException();

					if (taskFile == null)
					{
						taskFile = new TaskFileModel
						{
							Creator = CurrentUserToMember(project),
							CreationTime = DateTime.UtcNow,
							ContentType = file.ContentType,
							Size = content.Length,
							Owner = task
						};
						task.Files.Add(taskFile);
					}
					taskFile.Name = fileName;
					taskFile.LastChanger = CurrentUserToMember(project);
					taskFile.LastChangeTime = DateTime.UtcNow;

					SecurityService.DemandUpdate(taskFile, project, CurrentUser.Id, Session);

					_CrudDao.Store(taskFile);//must be called before store-need's file id for file name

					var relativeFilePath = taskFile.Path;
					if (relativeFilePath.IsNullOrWhiteSpace())
					{
						var ext = Path.GetExtension(fileName);
						var newFileName = taskFile.Id + ext;
						var rndFolder = new Random().Next(1000).ToString("D3");
						relativeFilePath = Path.Combine(project, rndFolder, newFileName);
					}
					//overwrite existing file, if any
					var absFilePath = Path.Combine(fileStoreRoot, relativeFilePath);
					var absDir = Path.GetDirectoryName(absFilePath);
					if (!Directory.Exists(absDir))
						Directory.CreateDirectory(absDir);
					using (var f = File.Open(absFilePath, FileMode.Create))
					{
						content.CopyTo(f);
						f.Close();
					}
					
					taskFile.Path = relativeFilePath;
					taskFile.Uploaded = true;

					result[idx] = new UploadResult<FileDTO>
					{
						Name = taskFile.Name,
						Length = taskFile.Size,
						Type = file.ContentType,
						Model = adapter.Fill(taskFile)
					};
				});
		    }

		    return new UploadedFiles<FileDTO> {Files = result};
	    }

	    private IModelFilterNode MakeFilesPredicate(string project, string numpp, IEnumerable<IModelFilterNode> filters)
	    {
			var fb = _FilteringService.Filter<TaskFileModel>();
			IModelFilterNode ownerPredicate = fb.Where(m => m.Owner.ProjectCode == project && m.Owner.SeqNumber == numpp);
			return SecurityService.ApplyReadConstraint<TaskFileModel>(
				project, CurrentUser.Id, Session, filters.Concat(new [] { ownerPredicate }).ToArray());
	    }

		[JsonEndpoint, RequireAuthorization]
	    public IEnumerable<FileDTO> GetFiles(
		    [NotEmpty] string project,
		    [NotEmpty] string ownerId,
		    [NotNull] ICollection<IModelFilterNode> filter,
		    [NotNull] ICollection<SortInfo> sorters,
		    [InRange(0, null)] int page)
	    {
		    var securedPredicate = MakeFilesPredicate(project, ownerId, filter);
		    var adapter = new FileAdapter();

		    return _FilteringDao.List<TaskFileModel>(securedPredicate, page, sorters)
			    .Select(adapter.Fill)
				.ToArray();
	    }

		[JsonEndpoint, RequireAuthorization]
		public int GetFilesCount(
			[NotEmpty] string project,
			[NotEmpty] string ownerId,
			[NotNull] ICollection<IModelFilterNode> filter)
		{
			var securedPredicate = MakeFilesPredicate(project, ownerId, filter);
			return _FilteringDao.RowCount<TaskFileModel>(securedPredicate);
		}
		
		[JsonEndpoint, RequireAuthorization]
	    public void DeleteFile([NotEmpty] string project, [NotEmpty] Guid fileId)
	    {
			var file = _CrudDao.Get<TaskFileModel>(fileId, true);

			SecurityService.DemandUpdate(file.Owner, project, CurrentUser.Id, Session);
			SecurityService.DemandDelete(file, project, CurrentUser.Id, Session);

			file.Owner.Files.Remove(file);
			_CrudDao.Delete(file);
	    }

		[JsonEndpoint, RequireAuthorization]
	    public void DeleteFiles([NotEmpty] string project, [NotEmpty] ICollection<Guid> ids)
	    {
			var trn = Session.BeginTransaction();
			try
			{
				foreach (var id in ids)
					DeleteFile(project, id);

				trn.Commit();
			}
			catch (Exception)
			{
				trn.Rollback();
				throw;
			}
	    }

		[JsonEndpointAttribute, RequireAuthorizationAttribute]
		public IEnumerable<IModelMetadata> TaskFilesMetadata([NotEmpty] string project)
		{
			return MetadataForModelAndRelations<TaskFileModel>(project);
		}

		#region IFileResourceStorage implementation

		private readonly ProjectToModuleCache p2m = new ProjectToModuleCache(ModuleDescriptor.MODULE_CODE);

		public IFileResource FindFile(string project, Guid fileId)
		{
			if (!p2m.IsProjectInHandledModule(project, Session)) 
				return null;

			var fb = _FilteringService.Filter<TaskFileModel>();
			IModelFilterNode predicate = fb.Where(m => m.Owner.ProjectCode == project && m.Id == fileId);
			predicate = SecurityService.ApplyReadConstraint<TaskFileModel>(project, CurrentUser.Id, Session, predicate);

			var file = _FilteringDao.Find<TaskFileModel>(predicate);
			return file != null ? new TaskFileWrapper(fileStoreRoot, file) : null;
		}

		#endregion

		#endregion

		#region Time tracking

		[JsonEndpoint, RequireAuthorization]
	    public TimelogDTO TrackTime(
		    [NotEmpty] string project,
		    [NotEmpty] Guid taskId,
		    [NotEmpty, InRange(0, null, false)] decimal time,
		    string comment)
		{
			var task = SecureFind<TaskModel>(project, taskId);

			DemandUpdate(task, project);

			var entry = task.TrackTime(CurrentUser, time, comment);
			DemandUpdate(entry, project);
			_CrudDao.Store(entry);

			return TaskViewAdapter.ToTimelog(entry);
		}

		[JsonEndpoint, RequireAuthorization]
		public TimelogDTO UpdateTime(
			[NotEmpty] string project,
			[NotEmpty] Guid timeId,
			[NotEmpty, InRange(0, null, false)] decimal time,
			string comment)
		{
			var entry = _CrudDao.Get<TaskTimelogEntryModel>(timeId, true);
			DemandUpdate(entry.Task, project);
			DemandUpdate(entry, project);

			entry.Time = time;
			entry.Comment = comment;
			entry.LastChanger = CurrentUserToMember(project);
			entry.LastChangeTime = DateTime.UtcNow;
			_CrudDao.Store(entry);

			return TaskViewAdapter.ToTimelog(entry);
		}

		[JsonEndpoint, RequireAuthorization]
	    public void DeleteTime([NotEmpty] string project, [NotEmpty] Guid timeId)
	    {
		    var entry = _CrudDao.Get<TaskTimelogEntryModel>(timeId, true);

			DemandUpdate(entry.Task, project);
			DemandDelete(entry, project);

		    entry.Task.Timelog.Remove(entry);
			_CrudDao.Delete(entry);
	    }

		#endregion
	}
}
