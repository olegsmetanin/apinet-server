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
using AGO.Core.Model.Processing;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Home;
using AGO.Home.Model.Projects;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate.Criterion;

namespace AGO.Tasks.Controllers
{
    /// <summary>
    /// Контроллер работы с задачами модуля задач
    /// </summary>
    public class TasksController: AbstractController
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
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			var query = _SessionProvider.CurrentSession.QueryOver<TaskModel>()
				.Where(m => m.ProjectCode == project)
				.OrderBy(m => m.InternalSeqNumber).Asc.
				ThenBy(m => m.SeqNumber).Asc;
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.SeqNumber).IsLike(term, MatchMode.Anywhere);

			return query.Skip(page * pageSize).Take(pageSize).LookupModelsList(m => m.SeqNumber).ToArray();
		}

		private static BaseTaskDTO.Executor ToExecutor(TaskExecutorModel executor)
		{
			var u = executor.Executor.User;
			return new BaseTaskDTO.Executor
			{
			    Name = u.FIO,
			    Description = u.FullName + (u.Departments.Count > 0
			       		? " (" + string.Join("; ", u.Departments.Select(d => d.FullName)) + ")"
			       		: string.Empty)
			};
		}

		private static CustomParameterDTO ParamToDTO(TaskCustomPropertyModel param)
		{
			return new CustomParameterDTO
			       	{
			       		Id = param.Id,
			       		TypeName = param.PropertyType.FullName,
			       		ValueType = param.PropertyType.ValueType,
			       		Value = param.Value.ConvertSafe<string>(),
			       		ModelVersion = param.ModelVersion
			       	};
		}

		private static StatusHistoryDTO StatusHistoryToDTO(TaskModel task, IModelMetadata meta)
		{
			return new StatusHistoryDTO
			       	{
			       		Current = meta.EnumDisplayValue<TaskModel, TaskStatus>(m => m.Status, task.Status),
			       		History = task.StatusHistory
			       			.OrderBy(h => h.Start)
			       			.Select(h => new StatusHistoryDTO.StatusHistoryItemDTO
			       			             	{
			       			             		Status = meta.EnumDisplayValue<TaskModel, TaskStatus>(m => m.Status, h.Status),
			       			             		Start = h.Start,
			       			             		Finish = h.Finish,
			       			             		Author = (h.Creator != null ? h.Creator.FIO : null)
			       			             	})
			       			.ToArray(),
			       		Next = Enum.GetValues(typeof (TaskStatus)) //TODO это должно браться из workflow
			       			.OfType<TaskStatus>()
							.Where(en => en != task.Status)
							.OrderBy(en => (int)en)
			       			.Select(s => new LookupEntry
			       			             	{
			       			             		Id = s.ToString(),
			       			             		Text = meta.EnumDisplayValue<TaskModel, TaskStatus>(m => m.Status, s)
			       			             	})
			       			.ToArray()
			       	};
		}

		private StatusHistoryDTO CustomStatusHistoryToDTO(TaskModel task)
		{

			var query = _SessionProvider.CurrentSession.QueryOver<CustomTaskStatusModel>()
				.Where(m => m.ProjectCode == task.ProjectCode);
			if (task.CustomStatus != null)
				query = query.Where(m => m.Id != task.CustomStatus.Id);

			query = query.OrderBy(m => m.ViewOrder).Asc.ThenBy(m => m.Name).Asc;

			return new StatusHistoryDTO
			{
				Current = (task.CustomStatus != null ? task.CustomStatus.Name : null),
				History = task.CustomStatusHistory
					.OrderBy(h => h.Start)
					.Select(h => new StatusHistoryDTO.StatusHistoryItemDTO
					{
						Status = h.Status.Name,
						Start = h.Start,
						Finish = h.Finish,
						Author = (h.Creator != null ? h.Creator.FIO : null)
					})
					.ToArray(),
				Next = query.List<CustomTaskStatusModel>().Select(s => new LookupEntry { Id = s.Id.ToString(), Text = s.Name}).ToArray()
			};
		}

		[JsonEndpoint, RequireAuthorization]
		public IEnumerable<TaskListItemDTO> GetTasks(
			[NotEmpty] string project,
			[NotNull] ICollection<IModelFilterNode> filter,
			[NotNull] ICollection<SortInfo> sorters,
			[InRange(0, null)] int page,
			[InRange(0, MaxPageSize)] int pageSize)
		{
			pageSize = pageSize == 0 ? DefaultPageSize : pageSize;

			var projectPredicate = _FilteringService.Filter<TaskModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] { projectPredicate }).ToArray();
			var meta = _SessionProvider.ModelMetadata(typeof(TaskModel));

			return _FilteringDao.List<TaskModel>(predicate,
				new FilteringOptions { Skip = page * pageSize, Take = pageSize, Sorters = sorters })
				.Select(m => new TaskListItemDTO
				{
					Id = m.Id,
					SeqNumber = m.SeqNumber,
					TaskType = (m.TaskType != null ? m.TaskType.Name : string.Empty),
					Content = m.Content,
					Executors = m.Executors.Select(ToExecutor).ToArray(),
					DueDate = m.DueDate,
					Status = meta.EnumDisplayValue<TaskModel, TaskStatus>(mm => mm.Status, m.Status),
					CustomStatus = (m.CustomStatus != null ? m.CustomStatus.Name : string.Empty),
					ModelVersion = m.ModelVersion
				})
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskViewDTO GetTask([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = _SessionProvider.CurrentSession.QueryOver<TaskModel>()
				.Where(m => m.ProjectCode == project && m.SeqNumber == numpp)
				.SingleOrDefault();
			
			if (task == null)
				throw new NoSuchEntityException();

			var meta = _SessionProvider.ModelMetadata(typeof(TaskModel));
			return new TaskViewDTO
			       	{
			       		Id = task.Id,
			       		SeqNumber = task.SeqNumber,
			       		TaskType = (task.TaskType != null ? task.TaskType.Name : string.Empty),
			       		Content = task.Content,
			       		Executors = task.Executors.OrderBy(e => e.Executor.User.FullName).Select(ToExecutor).ToArray(),
			       		DueDate = task.DueDate,
			       		Status = meta.EnumDisplayValue<TaskModel, TaskStatus>(mm => mm.Status, task.Status),
			       		CustomStatus = (task.CustomStatus != null ? task.CustomStatus.Name : string.Empty),
						Priority = meta.EnumDisplayValue<TaskModel, TaskPriority>(mm => mm.Priority, task.Priority),
						StatusHistory = StatusHistoryToDTO(task, meta),
						CustomStatusHistory = CustomStatusHistoryToDTO(task),
						Parameters = task.CustomProperties.OrderBy(p => p.PropertyType.FullName).Select(ParamToDTO).ToArray(),
			       		ModelVersion = task.ModelVersion,
						Author = (task.Creator != null ? task.Creator.FIO : null),
						CreationTime = task.CreationTime
			       	};
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult CreateTask([NotEmpty] string project, [NotNull] CreateTaskDTO model)
		{
			if (_SessionProvider.CurrentSession.QueryOver<ProjectModel>()
				.Where(m => m.ProjectCode == project).RowCount() <= 0)
				throw new NoSuchProjectException();

			var vr = new ValidationResult();
			try
			{
				if (model.TaskType == default(Guid))
					vr.AddFieldErrors("TaskType", "Не задан тип задачи");
				if (model.Executors == null || model.Executors.Length <= 0)
					vr.AddFieldErrors("Executors", "Не заданы исполнители");
				if (!vr.Success)
					return vr;

				//FIXME: replace when authController will work without http context
				var currentUser = _SessionProvider.CurrentSession.QueryOver<UserModel>()
					.Where(m => m.Login == "admin@agosystems.com").SingleOrDefault();
					// _AuthController.CurrentUser();

				//FIXME: это надо переделать на sequence или свой аналог
				var predicate = _FilteringService.Filter<TaskModel>().WhereProperty(m => m.ProjectCode).Eq(project);
				var num = _FilteringService.CompileFilter(predicate, typeof (TaskModel))
				          	.SetProjection(Projections.Max<TaskModel>(m => m.InternalSeqNumber))
				          	.GetExecutableCriteria(_SessionProvider.CurrentSession)
				          	.UniqueResult<long?>().GetValueOrDefault(0) + 1;
					
				var task = new TaskModel
				           	{
				           		Creator = currentUser,
				           		ProjectCode = project,
								InternalSeqNumber = num,
								SeqNumber = "t0-" + num,
				           		TaskType = _CrudDao.Get<TaskTypeModel>(model.TaskType),
								DueDate = model.DueDate,
				           		Content = model.Content,
								Priority = model.Priority
				           	};

				if (model.CustomStatus.HasValue)
				{
					var cs = _CrudDao.Get<CustomTaskStatusModel>(model.CustomStatus.Value);
					task.ChangeCustomStatus(cs, _AuthController.CurrentUser());
					var history = new CustomTaskStatusHistoryModel
					              	{
					              		Creator = currentUser,
					              		Task = task,
					              		Status = cs,
					              		Start = DateTime.Now
					              	};
					task.CustomStatusHistory.Add(history);
				}

				foreach (var id in model.Executors)
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
					               		Creator = currentUser,
					               		Task = task,
					               		Executor = participant
					               	};
					task.Executors.Add(executor);
				}

				_ModelProcessingService.ValidateModelSaving(task, vr);
				if (!vr.Success)
					return vr;

				_CrudDao.Store(task);
			} 
			catch(Exception ex)
			{
				vr.AddErrors(_LocalizationService.MessageForException(ex));
			}
			return vr;
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
    }
}