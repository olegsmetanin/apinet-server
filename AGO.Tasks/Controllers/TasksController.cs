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
			[InRange(0, null)] int page)
		{
			var projectPredicate = _FilteringService.Filter<TaskModel>().Where(m => m.ProjectCode == project);
			var predicate = filter.Concat(new[] { projectPredicate }).ToArray();
			var adapter = new TaskListItemAdapter(_SessionProvider.ModelMetadata(typeof(TaskModel)));

			return _FilteringDao.List<TaskModel>(predicate, page, sorters)
				.Select(adapter.Fill)
				.ToArray();
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskListItemDetailsDTO GetTaskDetails([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = _CrudDao.Find<TaskModel>(q => q.Where(m => m.ProjectCode == project && m.SeqNumber == numpp));

			if (task == null)
				throw new NoSuchEntityException();

			var adapter = new TaskListItemDetailsAdapter(_SessionProvider.ModelMetadata(typeof(TaskModel)));
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public TaskViewDTO GetTask([NotEmpty] string project, [NotEmpty] string numpp)
		{
			var task = _CrudDao.Find<TaskModel>(q => q.Where(m => m.ProjectCode == project && m.SeqNumber == numpp));
			
			if (task == null)
				throw new NoSuchEntityException();

			var adapter = new TaskViewAdapter(_SessionProvider.ModelMetadata(typeof(TaskModel)), Session);
			return adapter.Fill(task);
		}

		[JsonEndpoint, RequireAuthorization]
		public ValidationResult CreateTask([NotEmpty] string project, [NotNull] CreateTaskDTO model)
		{
			if (!_CrudDao.Exists<ProjectModel>(q => q.Where(m => m.ProjectCode == project)))
				throw new NoSuchProjectException();

			return Edit<TaskModel>(default(Guid), project,
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

			            task.ChangeStatus(TaskStatus.NotStarted, task.Creator);

			            if (model.CustomStatus.HasValue)
			            {
			                var cs = _CrudDao.Get<CustomTaskStatusModel>(model.CustomStatus.Value);
			                task.ChangeCustomStatus(cs, task.Creator);
			            }

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

			        });
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