using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers;
using AGO.Core.Localization;
using AGO.Core.Model.Dictionary;
using AGO.Tasks.Model;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Базовый класс адаптера моделей задач
	/// </summary>
	public abstract class TaskAdapter<TDTO> : ModelAdapter<TaskModel, TDTO> where TDTO: BaseTaskDTO, new()
	{
		protected ILocalizationService Localization;

		protected TaskAdapter(ILocalizationService localizationService)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			Localization = localizationService;
		}

		protected string EnumLocalizedValue<T>(T value)
		{
			return Localization.MessageForType(typeof (T), value) ?? value.ToString();
		}

		protected LookupEntry EnumLookupEntry<T>(T value)
		{
			return new LookupEntry
			       	{
			       		Id = value.ToString(),
			       		Text = Localization.MessageForType(typeof (T), value) ?? value.ToString()
			       	};
		}

		protected static Executor ToExecutor(TaskExecutorModel executor)
		{
			var u = executor.Executor.User;
			return new Executor
			       	{
			       		Id = executor.Executor.Id.ToString(), //use participants instead of technical entity - TaskExecutorModel
			       		Name = u.FIO,
			       		Description = u.FullName + (u.Departments.Count > 0
			       		                            	? " (" + string.Join("; ", u.Departments.Select(d => d.FullName)) + ")"
			       		                            	: string.Empty)
			       	};
		}

		protected LookupEntry? CustomStatusEntry(CustomTaskStatusModel status)
		{
			return status != null 
				? new LookupEntry { Id = status.Id.ToString(), Text = status.Name } 
				: (LookupEntry?) null;
		}

		public override TDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.SeqNumber = model.SeqNumber;
			dto.TaskType = (model.TaskType != null ? model.TaskType.Name : string.Empty);
			dto.Content = model.Content;
			dto.Executors = model.Executors.OrderBy(e => e.Executor.User.FullName).Select(ToExecutor).ToArray();
			dto.DueDate = model.DueDate;
			dto.Status = EnumLocalizedValue(model.Status);
			dto.CustomStatus = CustomStatusEntry(model.CustomStatus).GetValueOrDefault().Text;

			return dto;
		}
	}

	/// <summary>
	/// Адаптер моделей задач для списка (реестра)
	/// </summary>
	public class TaskListItemAdapter: TaskAdapter<TaskListItemDTO>
	{
		public TaskListItemAdapter(ILocalizationService localizationService) : base(localizationService)
		{
		}
	}

	public class TaskListItemDetailsAdapter
	{
		private readonly ILocalizationService lc;

		public TaskListItemDetailsAdapter(ILocalizationService localizationService)
		{
			if (localizationService == null)
				throw new ArgumentNullException("localizationService");
			lc = localizationService;
		}

		private AgreementView ToAgreement(TaskAgreementModel agreement)
		{
			return new AgreementView
			       	{
			       		Agreemer = agreement.Agreemer.User.FIO,
			       		Done = agreement.Done
			       	};
		}

		public TaskListItemDetailsDTO Fill(TaskModel task)
		{
			return new TaskListItemDetailsDTO
			       	{
			       		Priority = (lc.MessageForType(typeof(TaskPriority), task.Priority) ?? task.Priority.ToString()),
			       		Content = task.Content,
			       		Note = task.Note,
			       		Agreements = task.Agreements.Select(ToAgreement).ToArray(),
			       		Files = new[] {"Invoice.docx", "Orders.xlsx"}
			       	};
		}
	}

	/// <summary>
	/// Адаптер моделей задач для формы сводной информации
	/// </summary>
	public class TaskViewAdapter: TaskAdapter<TaskViewDTO>
	{
		private readonly ISession session;

		public TaskViewAdapter(ILocalizationService localizationService, ISession session)
			: base(localizationService)
		{
			if (session == null)
				throw new ArgumentNullException("session");
			this.session = session;
		}

		public static CustomParameterTypeDTO ParamTypeToDTO(CustomPropertyTypeModel paramType)
		{
			return new CustomParameterTypeDTO
			       	{
			       		Id = paramType.Id,
			       		Text = paramType.FullName,
			       		ValueType = paramType.ValueType
			       	};
		}

		public static CustomParameterDTO ParamToDTO(CustomPropertyInstanceModel param)
		{
			return new CustomParameterDTO
			{
				Id = param.Id,
				Type = ParamTypeToDTO(param.PropertyType),
				Value = param.Value,
				ModelVersion = param.ModelVersion
			};
		}

		private LookupEntry? TaskStatusEntry(TaskStatus status)
		{
			return new LookupEntry
			       	{
			       		Id = status.ToString(), 
						Text = EnumLocalizedValue(status)
			       	};
		}

		private StatusHistoryDTO.StatusHistoryItemDTO[] ToHistory<TStatus>(
			IEnumerable<IStatusHistoryRecordModel<TaskModel, TStatus>> history,
			Func<TStatus, LookupEntry?> status)
		{
			return history
				.OrderByDescending(h => h.Start)
				.Select(h => new StatusHistoryDTO.StatusHistoryItemDTO
				             	{
				             		Status = status(h.Status).GetValueOrDefault(),
				             		Start = h.Start,
				             		Finish = h.Finish,
				             		Author = ToAuthor(h)
				             	})
				.ToArray();
		}

		private StatusHistoryDTO StatusHistoryToDTO(TaskModel task)
		{
			return new StatusHistoryDTO
			{
				Current = TaskStatusEntry(task.Status),
				History = ToHistory(task.StatusHistory, TaskStatusEntry),
				Next = Enum.GetValues(typeof(TaskStatus)) //TODO это должно браться из workflow
					.OfType<TaskStatus>()
					.Where(en => en != task.Status)
					.OrderBy(en => (int)en)
					.Select(s => new LookupEntry
					{
						Id = s.ToString(),
						Text = EnumLocalizedValue(s)
					})
					.ToArray()
			};
		}

		private StatusHistoryDTO CustomStatusHistoryToDTO(TaskModel task)
		{

			var query = session.QueryOver<CustomTaskStatusModel>()
				.Where(m => m.ProjectCode == task.ProjectCode);
			if (task.CustomStatus != null)
				query = query.Where(m => m.Id != task.CustomStatus.Id);

			query = query.OrderBy(m => m.ViewOrder).Asc.ThenBy(m => m.Name).Asc;

			return new StatusHistoryDTO
			{
				Current = CustomStatusEntry(task.CustomStatus),
				History = ToHistory(task.CustomStatusHistory, CustomStatusEntry),
				Next = query.List<CustomTaskStatusModel>().Select(s => new LookupEntry { Id = s.Id.ToString(), Text = s.Name }).ToArray()
			};
		}

		public static Agreement ToAgreement(TaskAgreementModel agreement)
		{
			return new Agreement
			{
				Id = agreement.Id,
				ModelVersion = agreement.ModelVersion,
				Agreemer = agreement.Agreemer.User.FIO,
				DueDate = agreement.DueDate,
				Done = agreement.Done,
				AgreedAt = agreement.AgreedAt,
				Comment = agreement.Comment
			};
		}

		public override TaskViewDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.TaskType = new LookupEntry {Id = model.TaskType.Id.ToString(), Text = model.TaskType.Name};
			dto.Status = EnumLookupEntry(model.Status);
			dto.CustomStatus = model.CustomStatus != null
				? new LookupEntry { Id = model.CustomStatus.Id.ToString(), Text = model.CustomStatus.Name }
				: (LookupEntry?) null;
			dto.Priority = EnumLookupEntry(model.Priority);
			dto.Agreements = model.Agreements.Select(ToAgreement).ToArray();
			dto.StatusHistory = StatusHistoryToDTO(model);
			dto.CustomStatusHistory = CustomStatusHistoryToDTO(model);
			dto.Parameters = model.CustomProperties.OrderBy(p => p.PropertyType.FullName).Select(ParamToDTO).ToArray();
			dto.Author = ToAuthor(model);
			dto.CreationTime = model.CreationTime;

			return dto;
		}
	}
}