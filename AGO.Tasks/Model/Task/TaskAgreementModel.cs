﻿using System;
using System.Globalization;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
	/// <summary>
	/// Запись о согласовании задачи
	/// </summary>
	public class TaskAgreementModel : SecureModel<Guid>, ITasksModel
	{
		#region Persistent

		/// <summary>
		/// Согласуемая задача
		/// </summary>
		[JsonProperty, NotNull]
		public virtual TaskModel Task { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskId { get; set; }

		/// <summary>
		/// Согласующий (участник проекта)
		/// </summary>
		[JsonProperty, NotNull]
		public virtual ProjectMemberModel Agreemer { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? AgreemerId { get; set; }

		/// <summary>
		/// Согласовать до (срок согласования)
		/// </summary>
		[JsonProperty]
		public virtual DateTime? DueDate { get; set; }

		/// <summary>
		/// Дата согласования
		/// </summary>
		[JsonProperty]
		public virtual DateTime? AgreedAt { get; set; }

		/// <summary>
		/// Признак положительного согласования
		/// </summary>
		[JsonProperty]
		public virtual bool Done { get; set; }

		/// <summary>
		/// Комментарий
		/// </summary>
		[JsonProperty]
		public virtual string Comment { get; set; }

		#endregion

		/// <summary>
		/// Согласование просрочено
		/// </summary>
		[NotMapped, JsonProperty]
		public virtual bool IsExpired
		{
			get { return !Done && DueDate.HasValue && DueDate.Value > DateTime.Today; }
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", Agreemer, DueDate != null 
				? ": " + DueDate.Value.ToString("d", CultureInfo.CurrentUICulture) : string.Empty);
		}
	}
}