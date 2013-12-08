using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Filters.Metadata;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Processing;
using AGO.Core.Model.Projects;
using AGO.Core.Modules.Attributes;
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
    public class TasksController: AbstractTasksController
    {
        public TasksController(
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

		private IEnumerable<IModelFilterNode> MakeTasksPredicate(string project, IEnumerable<IModelFilterNode> filter, TaskPredefinedFilter predefined)
		{
			IModelFilterNode projectPredicate = _FilteringService.Filter<TaskModel>().Where(m => m.ProjectCode == project);
			IModelFilterNode predefinedPredicate = null;
			if (predefined != TaskPredefinedFilter.All)
			{
				var today = DateTime.Today;
				var f = _FilteringService.Filter<TaskModel>();

				switch (predefined)
				{
					case TaskPredefinedFilter.Overdue:
						{
							var tomorrow = today.AddDays(1);
							predefinedPredicate = f.Where(m => m.Status != TaskStatus.Closed && m.DueDate < tomorrow);
						}
						break;
					case TaskPredefinedFilter.DayLeft:
						{
							var dayAfterTomorrow = today.AddDays(2);
							predefinedPredicate = f.Where(m => m.Status != TaskStatus.Closed && m.DueDate < dayAfterTomorrow);
						}
						break;
					case TaskPredefinedFilter.WeekLeft:
						{
							var weekLater = today.AddDays(8);
							predefinedPredicate = f.Where(m => m.Status != TaskStatus.Closed && m.DueDate < weekLater);
						}
						break;
					case TaskPredefinedFilter.NoLimit:
						predefinedPredicate = f.WhereProperty(m => m.DueDate).Not().Exists();
						break;
					case TaskPredefinedFilter.ClosedToday:
						{
							var tomorrow = today.AddDays(1);
							predefinedPredicate = f.And()
								.Where(m => m.Status == TaskStatus.Closed)
								.WhereCollection(m => m.StatusHistory)
								.Where(m => m.Status == TaskStatus.Closed && m.Start >= today && m.Start < tomorrow).End();
						}
						break;
					case TaskPredefinedFilter.ClosedYesterday:
						var yesterday = today.AddDays(-1);
						predefinedPredicate = f.And()
							.Where(m => m.Status == TaskStatus.Closed)
							.WhereCollection(m => m.StatusHistory)
							.Where(m => m.Status == TaskStatus.Closed && m.Start >= yesterday && m.Start < today).End();
						break;
					default:
						throw new ArgumentOutOfRangeException("predefined");
				}
			}
			var predicate = filter.Concat(new[] { projectPredicate, predefinedPredicate }).ToArray();
			return predicate;
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskListItemDetailsDTO GetTaskDetails([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = _CrudDao.Find<TaskModel>(q => q.Where(m => m.ProjectCode == project && m.SeqNumber == numpp));

			if (task == null)
				throw new NoSuchEntityException();

			var adapter = new TaskListItemDetailsAdapter(_LocalizationService);
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskViewDTO GetTask([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = _CrudDao.Find<TaskModel>(q => q.Where(m => m.ProjectCode == project && m.SeqNumber == numpp));
			
			if (task == null)
				throw new NoSuchEntityException();

			var adapter = new TaskViewAdapter(_LocalizationService);
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<string> CreateTask([NotEmpty] string project, [NotNull] CreateTaskDTO model)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			return Edit<TaskModel, string>(default(Guid), project,
				(task, vr) =>
					{
						if (model.TaskType == default(Guid))
							vr.AddFieldErrors("TaskType", "Не задан тип задачи");
						if (model.Executors == null || model.Executors.Length <= 0)
							vr.AddFieldErrors("Executors", "Не заданы исполнители");
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
			                var participant = _CrudDao.Get<ProjectParticipantModel>(id);
			                if (participant == null)
			                {
			                    vr.AddFieldErrors("Executors", "Участник проекта по заданному идентификатору не найден");
			                    continue;
			                }
			                //TODO это лучше бы делать через _CrudDao.Get<>(project, id), а не так проверять
			                if (participant.Project.ProjectCode != task.ProjectCode)
			                {
			                    vr.AddFieldErrors("Executors", "Не учавствует в проекте задачи");
			                    continue;
			                }

			                var executor = new TaskExecutorModel
			                       			{
			                       			    Creator = task.Creator,
			                       			    Task = task,
			                       			    Executor = participant
			                       			};
			                task.Executors.Add(executor);
			            }
					}, task => task.SeqNumber);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<TaskViewDTO> UpdateTask([NotEmpty] string project, [NotNull] TaskPropChangeDTO data)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			return Edit<TaskModel, TaskViewDTO>(data.Id, project,
			    (task, vr) =>
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
								case "Content":
									task.Content = (string)data.Value;
									break;
								case "Note":
									task.Note = (string)data.Value;
									break;
								case "Priority":
									task.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), (string)data.Value);
									break;
								case "Status":
									var newStatus = (TaskStatus)Enum.Parse(typeof(TaskStatus), (string)data.Value);
									task.ChangeStatus(newStatus, _AuthController.CurrentUser());
									break;
								case "TaskType":
									task.TaskType = _CrudDao.Get<TaskTypeModel>(data.Value.ConvertSafe<Guid>(), true);
									break;
								case "DueDate":
									var dd = data.Value.ConvertSafe<DateTime?>();
									if (dd != null && dd.Value < DateTime.Today.ToUniversalTime())
									{
										//TODO localization??
										vr.AddFieldErrors("DueDate", "Due date can't be before today");
										break;
									}
									task.DueDate = dd;
									break;
								case "Executors":
									var ids = (data.Value.ConvertSafe<JArray>() ?? new JArray())
										.Select(id => id.ConvertSafe<Guid>()).ToArray();
									var toRemove = task.Executors.Where(e => !ids.Contains(e.Executor.Id)).ToArray();
									var toAdd = ids.Where(id => task.Executors.All(e => !e.Executor.Id.Equals(id)))
										.Select(id => _CrudDao.Get<ProjectParticipantModel>(id, true));

									foreach (var removed in toRemove)
									{
										task.Executors.Remove(removed);
										_CrudDao.Delete(removed);
									}
									foreach (var added in toAdd)
									{
										var executor = new TaskExecutorModel
										         	{
										         		Creator = _AuthController.CurrentUser(),
										         		Task = task,
										         		Executor = added
										         	};
										task.Executors.Add(executor);
										_CrudDao.Store(executor);
									}
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
					task => new TaskViewAdapter(_LocalizationService).Fill(task),
					() => { throw new TaskCreationNotSupportedException(); });
		}

		[JsonEndpoint, RequireAuthorization]
		public Agreement AddAgreemer([NotEmpty] Guid taskId, [NotEmpty] Guid participantId, DateTime? dueDate = null)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			var participant = _CrudDao.Get<ProjectParticipantModel>(participantId, true);

			if (task.Status == TaskStatus.Closed)
				throw new CanNotAddAgreemerToClosedTaskException();

			if (task.IsAgreemer(participant))
				throw new AgreemerAlreadyAssignedToTaskException(participant.User.FIO, task.SeqNumber);

			var agreement = new TaskAgreementModel
				                {
				                	Creator = _AuthController.CurrentUser(),
				                	Task = task,
				                	Agreemer = participant,
									DueDate = dueDate
				                };
			task.Agreements.Add(agreement);

			_CrudDao.Store(agreement);
			_CrudDao.Store(task);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool RemoveAgreement([NotEmpty] Guid taskId, [NotEmpty] Guid agreementId)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			var agreement = task.Agreements.FirstOrDefault(a => a.Id == agreementId);

			if (agreement == null) return false;
			if (task.Status == TaskStatus.Closed)
				throw new CanNotRemoveAgreemerFromClosedTaskException();
					
			//TODO check security rules, task status etc.

			task.Agreements.Remove(agreement);

			_CrudDao.Store(task);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public Agreement AgreeTask([NotEmpty] Guid taskId, string comment)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			var cu = _AuthController.CurrentUser();
			var agreement = task.Agreements.FirstOrDefault(a => a.Agreemer.User.Id == cu.Id);

			if (agreement == null)
				throw new CurrentUserIsNotAgreemerInTaskException();
			if (task.Status == TaskStatus.Closed)
				throw new CanNotAgreeClosedTaskException();

			//TODO check security rules, task status etc.
			agreement.Done = true;
			agreement.AgreedAt = DateTime.Now;
			agreement.Comment = comment;

			_CrudDao.Store(agreement);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		[JsonEndpoint, RequireAuthorization]
		public Agreement RevokeAgreement([NotEmpty] Guid taskId)
		{
			var task = _CrudDao.Get<TaskModel>(taskId, true);
			var cu = _AuthController.CurrentUser();
			var agreement = task.Agreements.FirstOrDefault(a => a.Agreemer.User.Id == cu.Id);

			if (agreement == null)
				throw new CurrentUserIsNotAgreemerInTaskException();
			if (task.Status == TaskStatus.Closed)
				throw new CanNotRevokeAgreementFromClosedTaskException();

			//TODO check security rules, task status etc.
			agreement.Done = false;
			agreement.AgreedAt = null;
			agreement.Comment = null;

			_CrudDao.Store(agreement);

			return TaskViewAdapter.ToAgreement(agreement);
		}

		private void InternalDeleteTask(Guid id)
		{
			var task = _CrudDao.Get<TaskModel>(id, true);

			//TODO: security and other checks

			_CrudDao.Delete(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTask([NotEmpty] Guid id)
		{
			InternalDeleteTask(id);

			return true;
		}

		[JsonEndpoint, RequireAuthorization]
		public bool DeleteTasks([NotEmpty] string project, [NotNull] ICollection<Guid> ids)
		{
			var s = _SessionProvider.CurrentSession;
			var trn = s.BeginTransaction();
			try
			{
				//TODO use project for security
				foreach (var id in ids)
					InternalDeleteTask(id);

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
		public IEnumerable<IModelMetadata> TaskMetadata()
		{
			return MetadataForModelAndRelations<TaskModel>();
		}

		[JsonEndpoint, RequireAuthorization]
    	public IEnumerable<CustomParameterTypeDTO> LookupParamTypes(
			[NotEmpty] string project, 
			string term, 
			[InRange(0, null)] int page)
		{
			var query = Session.QueryOver<CustomPropertyTypeModel>()
				.Where(m => m.ProjectCode == project);
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.FullName).IsLike(term, MatchMode.Anywhere);
			query = query.OrderBy(m => m.FullName).Asc;

			return _CrudDao.PagedQuery(query, page)
				.List<CustomPropertyTypeModel>()
				.Select(TaskViewAdapter.ParamTypeToDTO);
		}

		[JsonEndpoint, RequireAuthorization]
		public UpdateResult<CustomParameterDTO> EditParam([NotEmpty] Guid taskId, [NotNull] CustomParameterDTO model)
		{
			var task = _CrudDao.Get<TaskModel>(taskId);
			return Edit(model.Id, task.ProjectCode,
			    (param, vr) =>
			    {
			    	param.Value = model.Value;

			    }, TaskViewAdapter.ParamToDTO, 
				() => new TaskCustomPropertyModel
				      	{
				      		Creator = _AuthController.CurrentUser(),
							Task = task,
							PropertyType = _CrudDao.Get<CustomPropertyTypeModel>(model.Type.Id, true)
				      	});
		}
			
			
		[JsonEndpoint, RequireAuthorization]
		public bool DeleteParam([NotEmpty] Guid paramId)
		{
			//TODO security by task
			var param = _CrudDao.Get<TaskCustomPropertyModel>(paramId, true);
			param.Task.CustomProperties.Remove(param);
			_CrudDao.Delete(param);
			return true;
		}
    }
}