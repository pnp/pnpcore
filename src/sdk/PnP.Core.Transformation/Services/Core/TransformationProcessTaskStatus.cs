using System;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the status of a single task of a process
    /// </summary>
    public class TransformationProcessTaskStatus
    {
        /// <summary>
        /// Creates an instance for pending state
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="id"></param>
        /// <param name="creationDate"></param>
        public TransformationProcessTaskStatus(Guid processId, Guid id, DateTimeOffset creationDate)
        {
            ProcessId = processId;
            Id = id;
            CreationDate = creationDate;
            State = TransformationTaskExecutionState.Pending;
        }

        /// <summary>
        /// Creates an instance specifying the state
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="id"></param>
        /// <param name="creationDate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="state"></param>
        public TransformationProcessTaskStatus(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, TransformationTaskExecutionState state)
        {
            ProcessId = processId;
            Id = id;
            CreationDate = creationDate;
            StartDate = startDate;
            EndDate = endDate;
            State = state;
        }

        /// <summary>
        /// Creates an instance for faulted state
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="id"></param>
        /// <param name="creationDate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="exceptionMessage"></param>
        /// <param name="exceptionStackTrace"></param>
        public TransformationProcessTaskStatus(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, string exceptionMessage, string exceptionStackTrace)
        {
            ProcessId = processId;
            Id = id;
            CreationDate = creationDate;
            StartDate = startDate;
            EndDate = endDate;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
            State = TransformationTaskExecutionState.Faulted;
        }

        /// <summary>
        /// Creates an instance for faulted state
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="id"></param>
        /// <param name="creationDate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="exception"></param>
        public TransformationProcessTaskStatus(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            ProcessId = processId;
            Id = id;
            CreationDate = creationDate;
            StartDate = startDate;
            EndDate = endDate;
            ExceptionMessage = exception.Message;
            ExceptionStackTrace = exception.StackTrace;
            State = TransformationTaskExecutionState.Faulted;
        }

        /// <summary>
        /// Gets the id of the process
        /// </summary>
        public Guid ProcessId { get; }

        /// <summary>
        /// Gets the id of the task
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the creation date of the task
        /// </summary>
        public DateTimeOffset CreationDate { get; }

        /// <summary>
        /// Gets the date when task state became running
        /// </summary>
        public DateTimeOffset? StartDate { get; }

        /// <summary>
        /// Gets the date when task state became completed or faulted
        /// </summary>
        public DateTimeOffset? EndDate { get; }

        /// <summary>
        /// Gets the exception stack trace in case the state is Faulted
        /// </summary>
        public string ExceptionMessage { get; }

        /// <summary>
        /// Gets the exception stack trace in case the state is Faulted
        /// </summary>
        public string ExceptionStackTrace { get; }

        /// <summary>
        /// Gets the state of the task
        /// </summary>
        public TransformationTaskExecutionState State { get; }
    }
}