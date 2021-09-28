namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Object defining filters and sorting criteria for transformation tasks
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