using System;

namespace PnP.Core
{
    /// <summary>
    /// PnP Client exception
    /// </summary>
    public class ClientException : PnPException
    {
        #region Default exception constructors

        /// <summary>
        /// Default constructor for ClientException
        /// </summary>
        internal ClientException()
        {
        }

        /// <summary>
        /// Constructor for ClientException
        /// </summary>
        /// <param name="message">The message of the exception</param>
        internal ClientException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor for ClientException
        /// </summary>
        /// <param name="message">The message of the exception</param>
        /// <param name="innerException">The inner exception</param>
        internal ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
        #endregion

        #region Custom constructors
        /// <summary>
        /// <see cref="ClientException"/> constructor 
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="message">Error message</param>
        public ClientException(ErrorType type, string message) : base(message)
        {
            Error = new ClientError(type, message);
        }

        /// <summary>
        /// <see cref="ClientException"/> constructor 
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception to link to this exception</param>
        public ClientException(ErrorType type, string message, Exception innerException) : base(message, innerException)
        {
            Error = new ClientError(type, message);
        }
        #endregion

        /// <summary>
        /// Outputs a <see cref="ClientException"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Error?.ToString()}{Environment.NewLine}{base.ToString()}";
        }
    }
}
