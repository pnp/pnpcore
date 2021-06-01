namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    ///     List of process status
    /// </summary>
    public enum TransformationTaskExecutionState
    {
        /// <summary>
        ///     Process is in pending state
        /// </summary>
        Pending,

        /// <summary>
        ///     Process is running
        /// </summary>
        Running,

        /// <summary>
        ///     Process has been aborted
        /// </summary>
        Aborted,

        /// <summary>
        ///     Process is completed
        /// </summary>
        Completed,

        /// <summary>
        ///     Process could not process any tasks
        /// </summary>
        Faulted
    }
}