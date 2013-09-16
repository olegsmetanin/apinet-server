namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Не начата
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// В работе
        /// </summary>
        InWork = 1,

        /// <summary>
        /// Выполнена
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Закрыта
        /// </summary>
        Closed = 3,

        /// <summary>
        /// Приостановлена
        /// </summary>
        Suspended = 4
    }
}