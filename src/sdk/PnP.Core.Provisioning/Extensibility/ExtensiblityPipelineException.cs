using System;

namespace PnP.Core.Provisioning.Extensibility
{
    /// <summary>
    /// Thrown when a custom extensibility handler fails during a provisioning run.
    /// </summary>
    public sealed class ExtensiblityPipelineException : Exception
    {
        /// <summary>
        /// Creates the exception with a system supplied message.
        /// </summary>
        public ExtensiblityPipelineException() : base()
        {
        }

        /// <summary>
        /// Creates the exception with the given message.
        /// </summary>
        /// <param name="message">A string that describes the exception.</param>
        public ExtensiblityPipelineException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with the given message and the exception that caused it.
        /// </summary>
        /// <param name="message">A string that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ExtensiblityPipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
