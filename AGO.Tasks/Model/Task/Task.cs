using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Задача - основная запись основного реестра модуля
    /// </summary>
    public class Task: SecureModel<Guid>
    {
        #region Persistent

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
        [DisplayName("История изменения статуса"), PersistentCollection]
        public virtual ISet<TaskStatusHistoryModel> StatusHistory
        {
            get { return statusHistoryStore; }
            set { statusHistoryStore = value; }
        }
        private ISet<TaskStatusHistoryModel> statusHistoryStore = new HashSet<TaskStatusHistoryModel>();

        #endregion
    }
}