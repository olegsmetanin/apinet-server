using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Задача - основная запись основного реестра модуля
    /// </summary>
    public class TaskModel: SecureModel<Guid>
    {
        #region Persistent

		/// <summary>
		/// Код проекта, к которому относится задача
		/// </summary>
		[DisplayName("Код проекта"), JsonProperty, NotEmpty, NotLonger(ProjectModel.PROJECT_CODE_SIZE)]
		public virtual string ProjectCode { get; set; }

        /// <summary>
        /// Номер п/п задачи
        /// </summary>
        [DisplayName("Номер п/п"), JsonProperty, NotEmpty, NotLonger(16)]
        public virtual string SeqNumber { get; set; }

        /// <summary>
        /// Внутренний номер-счетчик задачи, используется для сортировки (вместо номера п/п)
        /// </summary>
        [DisplayName("Счетчик номера п/п"), JsonProperty, NotNull]
        public virtual long InternalSeqNumber { get; set; }

        /// <summary>
        /// Статус задачи
        /// </summary>
        [DisplayName("Статус"), JsonProperty, EnumDisplayNames(new[]
		{
			"NotStarted", "Не начата",
			"InWork", "В работе",
			"Completed", "Выполнена",
            "Closed", "Закрыта",
            "Suspended", "Приостановлена"
		})]
        public virtual TaskStatus Status { get; set; }

        /// <summary>
        /// История изменения статуса задачи
        /// </summary>
        [DisplayName("История изменения статуса"), PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
        public virtual ISet<TaskStatusHistoryModel> StatusHistory
        {
            get { return statusHistoryStore; }
            set { statusHistoryStore = value; }
        }
        private ISet<TaskStatusHistoryModel> statusHistoryStore = new HashSet<TaskStatusHistoryModel>();

		/// <summary>
		/// Приоритет
		/// </summary>
		[DisplayName("Приоритет"), JsonProperty, EnumDisplayNames(new[]
		{
			"Low", "Низкий",
			"Normal", "Нормальный",
			"High", "Высокий"
		})]
		public virtual TaskPriority Priority { get; set; }

		/// <summary>
		/// Тип задачи
		/// </summary>
		[DisplayName("Тип задачи"), JsonProperty, NotNull]
		public virtual TaskTypeModel TaskType { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskTypeId { get; set; }

		/// <summary>
		/// Содержание (описание задачи)
		/// </summary>
		[DisplayName("Содержание"), JsonProperty, NotLonger(1024)]
		public virtual string Content { get; set; }

		/// <summary>
		/// Примечание
		/// </summary>
		[DisplayName("Примечание"), JsonProperty]
		public virtual string Note { get; set; }

		/// <summary>
		/// Срок исполнения
		/// </summary>
		[DisplayName("Срок исполнения"), JsonProperty]
		public virtual DateTime? DueDate { get; set; }

		/// <summary>
		/// Пользовательский статус задачи
		/// </summary>
		[DisplayName("Пользовательский статус"), JsonProperty]
		public virtual CustomTaskStatusModel CustomStatus { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? CustomStatusId { get; set; }

		/// <summary>
		/// История изменения пользовательского статуса задачи
		/// </summary>
		[DisplayName("История изменения пользовательского статуса"), PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
		public virtual ISet<CustomTaskStatusHistoryModel> CustomStatusHistory
		{
			get { return customStatusHistoryStore; }
			set { customStatusHistoryStore = value; }
		}
		private ISet<CustomTaskStatusHistoryModel> customStatusHistoryStore = new HashSet<CustomTaskStatusHistoryModel>();

		/// <summary>
		/// Исполнители задачи
		/// </summary>
		[DisplayName("Исполнители"), PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
    	public virtual ISet<TaskExecutorModel> Executors
    	{
    		get { return executorsStore; }
    		set { executorsStore = value; }
    	}
    	private ISet<TaskExecutorModel> executorsStore = new HashSet<TaskExecutorModel>();

		/// <summary>
		/// Согласования задачи
		/// </summary>
		[DisplayName("Согласования"), PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
    	public virtual ISet<TaskAgreementModel> Agreements
    	{
			get { return agreementsStore; }
			set { agreementsStore = value; }
    	}
    	private ISet<TaskAgreementModel> agreementsStore = new HashSet<TaskAgreementModel>();

    	#endregion
    }
}