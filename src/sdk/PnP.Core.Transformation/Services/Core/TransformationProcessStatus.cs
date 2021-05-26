using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the status of a transformation execution process
    /// </summary>
    public class TransformationProcessStatus
    {
        private TransformationProcessStatus()
        {
        }

        /// <summary>
        /// Gets the process status as pending
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static TransformationProcessStatus GetPending(Guid processId) =>
            new TransformationProcessStatus {ProcessId = processId, Status = TransformationExecutionStatus.Pending};

        /// <summary>
        /// Gets the process status as running
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="done"></param>
        /// <param name="errors"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static TransformationProcessStatus GetRunning(Guid processId, int done, int errors, int? total) =>
            new TransformationProcessStatus { ProcessId = processId, Done = done, Errors = errors, Total = total, Status = TransformationExecutionStatus.Running };

        /// <summary>
        /// Gets the process status as completed
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="done"></param>
        /// <param name="errors"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static TransformationProcessStatus GetCompleted(Guid processId, int done, int errors, int? total) =>
            new TransformationProcessStatus { ProcessId = processId, Done = done, Errors = errors, Total = total, Status = TransformationExecutionStatus.Completed };

        /// <summary>
        /// Gets the process status as aborted
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="done"></param>
        /// <param name="errors"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static TransformationProcessStatus GetAborted(Guid processId, int done, int errors, int? total) =>
            new TransformationProcessStatus { ProcessId = processId, Done = done, Errors = errors, Total = total, Status = TransformationExecutionStatus.Aborted };

        /// <summary>
        /// The ID of the process
        /// </summary>
        public Guid ProcessId { get; private set; }

        /// <summary>
        /// Gets the number of total items
        /// </summary>
        public int? Total { get; private set; }

        /// <summary>
        /// Gets the number of done items
        /// </summary>
        public int Done { get; private set; }

        /// <summary>
        /// Gets the number of items with an error
        /// </summary>
        public int Errors { get; private set; }

        /// <summary>
        /// Gets the percentage of done items
        /// </summary>
        public int? Percentage => Total.HasValue ? Done * 100 / Total : null;

        /// <summary>
        /// Gets the status of the process
        /// </summary>
        public TransformationExecutionStatus Status { get; private set; }
    }

    /// <summary>
    /// List of process status
    /// </summary>
    public enum TransformationExecutionStatus
    {
        /// <summary>
        /// Process is in pending state
        /// </summary>
        Pending,
        /// <summary>
        /// Process is running
        /// </summary>
        Running,
        /// <summary>
        /// Process has been aborted
        /// </summary>
        Aborted,
        /// <summary>
        /// Process is completed
        /// </summary>
        Completed
    }
}
