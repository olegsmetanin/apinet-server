﻿using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters.Metadata;
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
		protected IModelMetadata Meta;

		protected TaskAdapter(IModelMetadata meta)
		{
			if (meta == null)
				throw new ArgumentNullException("meta");
			Meta = meta;
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

		protected string CustomStatusDescription(CustomTaskStatusModel status)
		{
			return status != null ? status.Name : string.Empty;
		}

		public override TDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.SeqNumber = model.SeqNumber;
			dto.TaskType = (model.TaskType != null ? model.TaskType.Name : string.Empty);
			dto.Content = model.Content;
			dto.Executors = model.Executors.OrderBy(e => e.Executor.User.FullName).Select(ToExecutor).ToArray();
			dto.DueDate = model.DueDate;
			dto.Status = Meta.EnumDisplayValue<TaskModel, TaskStatus>(mm => mm.Status, model.Status);
			dto.CustomStatus = CustomStatusDescription(model.CustomStatus);

			return dto;
		}
	}

	/// <summary>
	/// Адаптер моделей задач для списка (реестра)
	/// </summary>
	public class TaskListItemAdapter: TaskAdapter<TaskListItemDTO>
	{
		public TaskListItemAdapter(IModelMetadata meta) : base(meta)
		{
		}
	}

	public class TaskListItemDetailsAdapter
	{
		private readonly IModelMetadata meta;

		public TaskListItemDetailsAdapter(IModelMetadata meta)
		{
			if (meta == null)
				throw new ArgumentNullException("meta");
			this.meta = meta;
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
			       		Priority = meta.EnumDisplayValue<TaskModel, TaskPriority>(m => m.Priority, task.Priority),
			       		Content = task.Content,
			       		Note = task.Note,
			       		Agreements = task.Agreements
							.Select(ToAgreement)
							.Concat(new [] 
								{
									new AgreementView { Agreemer = "Пупкин В.В.", Done = true},
									new AgreementView { Agreemer = "Вовкин П.П."}
								}).ToArray(),
			       		Files = new[] {"Договор.docx", "Смета.xlsx", "Летчик.jpg"}
			       	};
		}
	}

	/// <summary>
	/// Адаптер моделей задач для формы сводной информации
	/// </summary>
	public class TaskViewAdapter: TaskAdapter<TaskViewDTO>
	{
		private readonly ISession session;

		public TaskViewAdapter(IModelMetadata meta, ISession session)
			: base(meta)
		{
			if (session == null)
				throw new ArgumentNullException("session");
			this.session = session;
		}

		private static CustomParameterDTO ParamToDTO(CustomPropertyInstanceModel param)
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

		private string TaskStatusDescription(TaskStatus status)
		{
			return Meta.EnumDisplayValue<TaskModel, TaskStatus>(m => m.Status, status);
		}

		private StatusHistoryDTO.StatusHistoryItemDTO[] ToHistory<TStatus>(
			IEnumerable<IStatusHistoryRecordModel<TaskModel, TStatus>> history,
			Func<TStatus, string> status)
		{
			return history
				.OrderByDescending(h => h.Start)
				.Select(h => new StatusHistoryDTO.StatusHistoryItemDTO
				             	{
				             		Status = status(h.Status),
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
				Current = TaskStatusDescription(task.Status),
				History = ToHistory(task.StatusHistory, TaskStatusDescription),
				Next = Enum.GetValues(typeof(TaskStatus)) //TODO это должно браться из workflow
					.OfType<TaskStatus>()
					.Where(en => en != task.Status)
					.OrderBy(en => (int)en)
					.Select(s => new LookupEntry
					{
						Id = s.ToString(),
						Text = Meta.EnumDisplayValue<TaskModel, TaskStatus>(m => m.Status, s)
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
				Current = CustomStatusDescription(task.CustomStatus),
				History = ToHistory(task.CustomStatusHistory, CustomStatusDescription),
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
				Done = agreement.Done,
				AgreedAt = agreement.AgreedAt,
				Comment = agreement.Comment
			};
		}

		public override TaskViewDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.TaskType = new LookupEntry {Id = model.TaskType.Id.ToString(), Text = model.TaskType.Name};
			dto.Status = Meta.EnumLookupEntry<TaskModel, TaskStatus>(mm => mm.Status, model.Status);
			dto.CustomStatus = model.CustomStatus != null
				? new LookupEntry { Id = model.CustomStatus.Id.ToString(), Text = model.CustomStatus.Name }
				: (LookupEntry?) null;
			dto.Priority = Meta.EnumLookupEntry<TaskModel, TaskPriority>(mm => mm.Priority, model.Priority);
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