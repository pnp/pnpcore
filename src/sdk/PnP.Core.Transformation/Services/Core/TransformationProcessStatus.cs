using System;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    ///     Defines the status of a transformation execution process
    /// </summary>
    public class TransformationProcessStatus
    {
        /// <summary>
        ///     Creates a new instance
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="state"></param>
        public TransformationProcessStatus(Guid processId, TransformationExecutionState state)
        {
            ProcessId = processId;
            State = state;
        }

        /// <summary>
        ///     The ID of the process
        /// </summary>
        public Guid ProcessId { get; }

        /// <summary>
        ///     Gets the status of the process
        /// </summary>
        public TransformationExecutionState State { get; }
    }

    /// <summary>
    ///     List of process status
    /// </summary>
    public enum TransformationExecutionState
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