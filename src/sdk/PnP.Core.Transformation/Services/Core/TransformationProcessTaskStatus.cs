using System;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the status of a single task of a process
    /// </summary>
    public class TransformationProcessTaskStatus
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="id">The task id</param>
        /// <param name="creationDate">The creation date time for the task</param>
        /// <param name="startDate">The start date time for the task</param>
        /// <param name="endDate">The end date time for the task</param>
        /// <param name="state">The state of the task</param>
        /// <param name="exceptionMessage">The exception message, if any</param>
        /// <param name="exceptionStackTrace">The exception stack trace, if any</param>
        public TransformationProcessTaskStatus(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, TransformationTaskExecutionState state, string exceptionMessage, string exceptionStackTrace)
        {
            ProcessId = processId;
            Id = id;
            CreationDate = creationDate;
            StartDate = startDate;
            EndDate = endDate;
            State = state;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
        }

        private TransformationProcessTaskStatus()
        {
            
        }

        /// <summary>
        /// Creates an instance for pending state
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="id">The task id</param>
        /// <param name="creationDate">The creation date time for the task</param>
        /// <returns>A new instance of  <see cref="TransformationProcessTaskStatus"/></returns>
        public static TransformationProcessTaskStatus CreatePending(Guid processId, Guid id, DateTimeOffset creationDate)
        {
            return new TransformationProcessTaskStatus
            {
                ProcessId = processId,
                Id = id,
                CreationDate = creationDate,
                State = TransformationTaskExecutionState.Pending
            };
        }

        /// <summary>
        /// Creates an instance specifying the state
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="id">The task id</param>
        /// <param name="creationDate">The creation date time for the task</param>
        /// <param name="startDate">The start date time for the task</param>
        /// <param name="endDate">The end date time for the task</param>
        /// <param name="state">The state of the task</param>
        /// <returns>A new instance of  <see cref="TransformationProcessTaskStatus"/></returns>
        public static TransformationProcessTaskStatus CreateNormal(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, TransformationTaskExecutionState state)
        {
            return new TransformationProcessTaskStatus
            {
                ProcessId = processId,
                Id = id,
                CreationDate = creationDate,
                StartDate = startDate,
                EndDate = endDate,
                State = state,
            };
        }

        /// <summary>
        /// Creates an instance for faulted state
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="id">The task id</param>
        /// <param name="creationDate">The creation date time for the task</param>
        /// <param name="startDate">The start date time for the task</param>
        /// <param name="endDate">The end date time for the task</param>
        /// <param name="exceptionMessage">The exception message, if any</param>
        /// <param name="exceptionStackTrace">The exception stack trace, if any</param>
        /// <returns>A new instance of  <see cref="TransformationProcessTaskStatus"/></returns>
        public static TransformationProcessTaskStatus CreateFaulted(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, string exceptionMessage, string exceptionStackTrace)
        {
            return new TransformationProcessTaskStatus
            {
                ProcessId = processId,
                Id = id,
                CreationDate = creationDate,
                StartDate = startDate,
                EndDate = endDate,
                ExceptionMessage = exceptionMessage,
                ExceptionStackTrace = exceptionStackTrace,
                State = TransformationTaskExecutionState.Faulted
            };
        }

        /// <summary>
        /// Creates an instance for faulted state
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="id">The task id</param>
        /// <param name="creationDate">The creation date time for the task</param>
        /// <param name="startDate">The start date time for the task</param>
        /// <param name="endDate">The end date time for the task</param>
        /// <param name="exception">The exception</param>
        /// <returns>A new instance of  <see cref="TransformationProcessTaskStatus"/></returns>
        public static TransformationProcessTaskStatus CreateFaulted(Guid processId, Guid id, DateTimeOffset creationDate, DateTimeOffset? startDate, DateTimeOffset? endDate, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new TransformationProcessTaskStatus
            {
                ProcessId = processId,
                Id = id,
                CreationDate = creationDate,
                StartDate = startDate,
                EndDate = endDate,
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace,
                State = TransformationTaskExecutionState.Faulted
            };
        }

        /// <summary>
        /// Gets the id of the process
        /// </summary>
        public Guid ProcessId { get; private set; }

        /// <summary>
        /// Gets the id of the task
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the creation date of the task
        /// </summary>
        public DateTimeOffset CreationDate { get; private set; }

        /// <summary>
        /// Gets the date when task state became running
        /// </summary>
        public DateTimeOffset? StartDate { get; private set; }

        /// <summary>
        /// Gets the date when task state became completed or faulted
        /// </summary>
        public DateTimeOffset? EndDate { get; private set; }

        /// <summary>
        /// Gets the exception stack trace in case the state is Faulted
        /// </summary>
        public string ExceptionMessage { get; private set; }

        /// <summary>
        /// Gets the exception stack trace in case the state is Faulted
        /// </summary>
        public string ExceptionStackTrace { get; private set; }

        /// <summary>
        /// Gets the state of the task
        /// </summary>
        public TransformationTaskExecutionState State { get; private set; }
    }
}