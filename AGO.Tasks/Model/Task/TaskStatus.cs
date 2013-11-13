namespace AGO.Tasks.Model.Task
{
    /// <summary>
    /// Статус задачи
    /// </summary>
    /// <remarks>
    /// Workflow:
    /// New -> Doing -> Done -> Closed
    ///           |-> Discarded -^
    /// </remarks>
    public enum TaskStatus
    {
        New = 0,

        Doing = 1,

        Done = 2,

        Discarded = 3,

        Closed = 4
    }
}