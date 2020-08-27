namespace PnP.Core
{
    /// <summary>
    /// Information about the client error
    /// </summary>
    public class ClientError : BaseError
    {
        /// <summary>
        /// ClientError constructor, creates a <see cref="ClientError"/> for the provided error type and message
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="message">Error message</param>
        public ClientError(ErrorType type, string message) : base(type)
        {
            Message = message;
        }

        /// <summary>
        /// Error message that was linked to the client error
        /// </summary>
        public string Message { get; internal set; }
    }
}
