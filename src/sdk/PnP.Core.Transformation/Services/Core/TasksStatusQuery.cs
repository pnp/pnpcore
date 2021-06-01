namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Object containing filters and orders criteria
    /// </summary>
    public class TasksStatusQuery
    {
        /// <summary>
        /// Creates an instance to identify a query which returns all items
        /// </summary>
        public TasksStatusQuery()
        { }

        /// <summary>
        /// Creates an instance to query only items with a specific state
        /// </summary>
        /// <param name="state"></param>
        public TasksStatusQuery(TransformationTaskExecutionState state)
        {
            State = state;
        }

        /// <summary>
        /// The optional state to filter by
        /// </summary>
        public TransformationTaskExecutionState? State { get; }

    }
}