﻿using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers;
using AGO.Core.Localization;
using AGO.Core.Model;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Task;
using AGO.Tasks.Workflow;

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

		protected static LookupEntry ToExecutor(TaskExecutorModel executor)
		{
			return new LookupEntry
			{
			    Id = executor.Executor.Id.ToString(), //use participants instead of technical entity - TaskExecutorModel
			    Text = executor.Executor.FullName
			};
		}

		public static LookupEntry ToTag(TaskToTagModel tagLink)
		{
			return new LookupEntry
			       	{
			       		Id = tagLink.Tag.Id.ToString(),
			       		Text = tagLink.Tag.FullName
			       	};
		}

		public override TDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.ProjectCode = model.ProjectCode;
			dto.SeqNumber = model.SeqNumber;
			dto.TaskType = (model.TaskType != null ? model.TaskType.Name : string.Empty);
			dto.Content = model.Content;
			dto.Executors = model.Executors.OrderBy(e => e.Executor.FullName).Select(ToExecutor).ToArray();
			dto.DueDate = model.DueDate;
			dto.Status = EnumLocalizedValue(model.Status);

			return dto;
		}
	}

	/// <summary>
	/// Адаптер моделей задач для списка (реестра)
	/// </summary>
	public class TaskListItemAdapter: TaskAdapter<TaskListItemDTO>
	{
		private readonly UserModel currentUser;

		public TaskListItemAdapter(ILocalizationService localizationService, UserModel currentUser) : base(localizationService)
		{
			if (currentUser == null)
				throw new ArgumentNullException("currentUser");

			this.currentUser = currentUser;
		}

		public override TaskListItemDTO Fill(TaskModel task)
		{
			var dto = base.Fill(task);
			//Только свои персональные теги
			var allowed = task.Tags.Where(tl => tl.Tag.OwnerId == currentUser.Id);
			dto.Tags = allowed.OrderBy(tl => tl.Tag.FullName).Select(ToTag).ToArray();
			return dto;
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
				Agreemer = agreement.Agreemer.FullName,
				Done = agreement.Done
			};
		}

		private LookupEntry ToFile(TaskFileModel file)
		{
			return new LookupEntry
			{
				Id = file.Id.ToString(),
				Text = file.Name
			};
		}

		public TaskListItemDetailsDTO Fill(TaskModel task)
		{
			return new TaskListItemDetailsDTO
			{
				Priority = (lc.MessageForType(typeof (TaskPriority), task.Priority) ?? task.Priority.ToString()),
				Content = task.Content,
				EstimatedTime = task.EstimatedTime,
				SpentTime = task.CalculateSpentTime(),
				Note = task.Note,
				Agreements = task.Agreements.Select(ToAgreement).ToArray(),
				Files = task.Files.OrderBy(f => f.CreationTime).Take(5).Select(ToFile).ToArray()
			};
		}
	}

	/// <summary>
	/// Адаптер моделей задач для формы сводной информации
	/// </summary>
	public class TaskViewAdapter: TaskAdapter<TaskViewDTO>
	{
	    private static IWorkflow<TaskStatus> workflow = new TaskStatusWorkflow();

		public TaskViewAdapter(ILocalizationService localizationService): base(localizationService)
		{
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
			IEnumerable<IStatusHistoryRecordModel<TaskModel, TStatus, ProjectMemberModel>> history,
			Func<TStatus, LookupEntry?> status)
		{
			return history
				.OrderByDescending(h => h.Start)
				.Select(h => new StatusHistoryDTO.StatusHistoryItemDTO
				             	{
				             		Status = status(h.Status).GetValueOrDefault(),
				             		Start = h.Start,
				             		Finish = h.Finish,
				             		Author = h.Creator.FullName
				             	})
				.ToArray();
		}

		private StatusHistoryDTO StatusHistoryToDTO(TaskModel task)
		{
			return new StatusHistoryDTO
			{
				Current = TaskStatusEntry(task.Status),
				History = ToHistory(task.StatusHistory, TaskStatusEntry),
				Next = workflow.Next(task.Status)
					.OrderBy(en => (int)en)
					.Select(s => new LookupEntry
					{
						Id = s.ToString(),
						Text = EnumLocalizedValue(s)
					})
					.ToArray()
			};
		}

		public static Agreement ToAgreement(TaskAgreementModel agreement)
		{
			return new Agreement
			{
				Id = agreement.Id,
				ModelVersion = agreement.ModelVersion,
				Agreemer = agreement.Agreemer.FullName,
				DueDate = agreement.DueDate,
				Done = agreement.Done,
				AgreedAt = agreement.AgreedAt,
				Comment = agreement.Comment
			};
		}

		public static TimelogDTO ToTimelog(TaskTimelogEntryModel entry)
		{
			return new TimelogDTO
			{
				Id = entry.Id,
				Member = entry.Member.FullName,
				Time = entry.Time,
				Comment = entry.Comment,
				Editor = entry.LastChanger != null ? entry.LastChanger.FullName : null,
				CreationTime = entry.CreationTime
			};
		}

		public override TaskViewDTO Fill(TaskModel model)
		{
			var dto = base.Fill(model);

			dto.TaskType = new LookupEntry {Id = model.TaskType.Id.ToString(), Text = model.TaskType.Name};
			dto.Status = EnumLookupEntry(model.Status);
			dto.Priority = EnumLookupEntry(model.Priority);
			dto.Note = model.Note;
			dto.Agreements = model.Agreements.Select(ToAgreement).ToArray();
			dto.StatusHistory = StatusHistoryToDTO(model);
			dto.Parameters = model.CustomProperties.OrderBy(p => p.PropertyType.FullName).Select(ParamToDTO).ToArray();
			dto.Author = ToAuthor(model);
			dto.CreationTime = model.CreationTime;
			dto.EstimatedTime = model.EstimatedTime;
			dto.SpentTime = model.CalculateSpentTime();
			dto.Timelog = model.Timelog.Select(ToTimelog).ToArray();

			return dto;
		}
	}
}