using System;

namespace PnP.Core
{
    /// <summary>
    /// PnP Client exception
    /// </summary>
    public class ClientException: PnPException
    {
        #region Default exception constructors
        internal ClientException()
        {
        }

        internal ClientException(string message) : base(message)
        {
        }

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
