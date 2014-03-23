using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model;
using AGO.Core.Model.Files;
using AGO.Core.Model.Security;
using AGO.Core.Model.Projects;
using AGO.Tasks.Model.Dictionary;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Задача - основная запись основного реестра модуля
    /// </summary>
	public class TaskModel : SecureProjectBoundModel<Guid>, ITasksModel, IFileOwner<TaskModel, TaskFileModel>
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

		/// <summary>
		/// Теги задачи (проектные и персональные)
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
    	public virtual ISet<TaskToTagModel> Tags
    	{
    		get { return tagsStore; }
			set { tagsStore = value; }
    	}
		private ISet<TaskToTagModel> tagsStore = new HashSet<TaskToTagModel>();

		/// <summary>
		/// Файлы задачи
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan, Column = "OwnerId")]
	    public virtual ISet<TaskFileModel> Files
	    {
		    get { return files; }
			set { files = value; }
	    }
		private ISet<TaskFileModel> files = new HashSet<TaskFileModel>();

		#region Time tracking

		/// <summary>
		/// Планируемое время выполнения задачи
		/// </summary>
		[InRange(0, null, false)]
		public virtual decimal? EstimatedTime { get; set; }

		/// <summary>
		/// Затраченное время на выполнение задачи
		/// </summary>
		[PersistentCollection(CascadeType = CascadeType.AllDeleteOrphan)]
	    public virtual ISet<TaskTimelogEntryModel> Timelog
	    {
			get { return timelogEntries; }
			set { timelogEntries = value; }
	    }

	    private ISet<TaskTimelogEntryModel> timelogEntries = new HashSet<TaskTimelogEntryModel>();

		#endregion

		#endregion

		public virtual TaskStatusHistoryModel ChangeStatus(TaskStatus newStatus, ProjectMemberModel changer)
		{
			return StatusChangeHelper.Change(this, newStatus, StatusHistory, changer);
		}

	    public virtual bool IsExecutor(ProjectMemberModel member)
	    {
		    return member != null && Executors.Any(e => member.Equals(e.Executor));
	    }

		public virtual bool IsAgreemer(ProjectMemberModel member)
		{
			return member != null && Agreements.Any(a => member.Equals(a.Agreemer));
		}

	    public virtual decimal? CalculateSpentTime()
	    {
		    return Timelog.Any() ? Timelog.Sum(e => e.Time) : (decimal?) null;
	    }

	    public virtual TaskTimelogEntryModel TrackTime(UserModel user, decimal time, string comment = null)
	    {
			if (user == null)
				throw new ArgumentNullException("user");
			if (time <= decimal.Zero)
				throw new ArgumentException("Time can not be less than zero", "time");

		    var executor = Executors.FirstOrDefault(e => e.Executor.UserId == user.Id);
			if (executor == null)
				throw new CurrentUserIsNotTaskExecutorException();

		    var entry = new TaskTimelogEntryModel
		    {
				Creator = executor.Executor,
				CreationTime = DateTime.UtcNow,
			    Task = this,
			    Member = executor.Executor,
			    Time = time,
			    Comment = comment
		    };
		    Timelog.Add(entry);
		    return entry;
	    }

		public override string ToString()
		{
			return SeqNumber.TrimSafe() ?? base.ToString();
		}
    }
}