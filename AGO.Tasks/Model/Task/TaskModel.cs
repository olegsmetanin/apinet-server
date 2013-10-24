using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using Newtonsoft.Json;
using System.Linq;

namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Задача - основная запись основного реестра модуля
    /// </summary>
	public class TaskModel : SecureProjectBoundModel<Guid>, ITasksModel
    {
        #region Persistent

        /// <summary>
        /// Номер п/п задачи
        /// </summary>
        [JsonProperty, NotEmpty, NotLonger(16)]
        public virtual string SeqNumber { get; set; }

        /// <summary>
        /// Внутренний номер-счетчик задачи, используется для сортировки (вместо номера п/п)
        /// </summary>
        [JsonProperty, NotNull]
        public virtual long InternalSeqNumber { get; set; }

        /// <summary>
        /// Статус задачи
        /// </summary>
        [JsonProperty]
        public virtual TaskStatus Status { get; protected set; }

        /// <summary>
        /// История изменения статуса задачи
        /// </summary>
        [PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
        public virtual ISet<TaskStatusHistoryModel> StatusHistory
        {
            get { return statusHistoryStore; }
            set { statusHistoryStore = value; }
        }
        private ISet<TaskStatusHistoryModel> statusHistoryStore = new HashSet<TaskStatusHistoryModel>();

		/// <summary>
		/// Приоритет
		/// </summary>
		[JsonProperty]
		public virtual TaskPriority Priority { get; set; }

		/// <summary>
		/// Тип задачи
		/// </summary>
		[JsonProperty, NotNull]
		public virtual TaskTypeModel TaskType { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? TaskTypeId { get; set; }

		/// <summary>
		/// Содержание (описание задачи)
		/// </summary>
		[JsonProperty, NotLonger(1024)]
		public virtual string Content { get; set; }

		/// <summary>
		/// Примечание
		/// </summary>
		[JsonProperty]
		public virtual string Note { get; set; }

		/// <summary>
		/// Срок исполнения
		/// </summary>
		[JsonProperty]
		public virtual DateTime? DueDate { get; set; }

		/// <summary>
		/// Пользовательский статус задачи
		/// </summary>
		[JsonProperty]
		public virtual CustomTaskStatusModel CustomStatus { get; protected set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? CustomStatusId { get; set; }

		/// <summary>
		/// История изменения пользовательского статуса задачи
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
		public virtual ISet<CustomTaskStatusHistoryModel> CustomStatusHistory
		{
			get { return customStatusHistoryStore; }
			set { customStatusHistoryStore = value; }
		}
		private ISet<CustomTaskStatusHistoryModel> customStatusHistoryStore = new HashSet<CustomTaskStatusHistoryModel>();

		/// <summary>
		/// Исполнители задачи
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan), NotEmpty]
    	public virtual ISet<TaskExecutorModel> Executors
    	{
    		get { return executorsStore; }
    		set { executorsStore = value; }
    	}
    	private ISet<TaskExecutorModel> executorsStore = new HashSet<TaskExecutorModel>();

		/// <summary>
		/// Согласования задачи
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
    	public virtual ISet<TaskAgreementModel> Agreements
    	{
			get { return agreementsStore; }
			set { agreementsStore = value; }
    	}
    	private ISet<TaskAgreementModel> agreementsStore = new HashSet<TaskAgreementModel>();

		/// <summary>
		/// Пользовательские свойства
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
    	public virtual ISet<TaskCustomPropertyModel> CustomProperties
    	{
    		get { return customPropsStore; }
			set { customPropsStore = value; }
    	}
    	private ISet<TaskCustomPropertyModel> customPropsStore = new HashSet<TaskCustomPropertyModel>();

    	#endregion

		public virtual TaskStatusHistoryModel ChangeStatus(TaskStatus newStatus, UserModel changer)
		{
			return StatusChangeHelper.Change(this, newStatus, StatusHistory, changer);
		}

		public virtual CustomTaskStatusHistoryModel ChangeCustomStatus(CustomTaskStatusModel newStatus, UserModel changer)
		{
			return StatusChangeHelper.Change(this, newStatus, CustomStatusHistory, changer, m => m.CustomStatus);
		}

		public virtual bool IsAgreemer(ProjectParticipantModel participant)
		{
			return participant != null && Agreements.Any(a => participant.Equals(a.Agreemer));
		}
    }
}