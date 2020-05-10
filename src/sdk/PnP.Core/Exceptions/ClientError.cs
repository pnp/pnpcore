namespace PnP.Core
{
    /// <summary>
    /// Information about the client error
    /// </summary>
    public class ClientError : BaseError
    {
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
