namespace AGO.Tasks.Workflow
{
    /// <summary>
    /// Interface for generic workflows (may be not needed, but if needed will be moved to core)
    /// </summary>
    /// <typeparam name="TStatus">Type of entity (mostly registry record) status</typeparam>
    public interface IWorkflow<TStatus>
    {
        /// <summary>
        /// Return next statuses, that available from <paramref name="current"/> by workflow rules
        /// </summary>
        /// <param name="current">Current status</param>
        /// <returns>Next statuses, available to move to</returns>
        TStatus[] Next(TStatus current);

        /// <summary>
        /// Determine, if transition from <paramref name="from"/> to <paramref name="to"/> is valid in accordance with workflow rules
        /// </summary>
        /// <param name="from">From status</param>
        /// <param name="to">To status</param>
        /// <returns>true, if valid transition</returns>
        bool IsValidTransitions(TStatus from, TStatus to);
    }
}
