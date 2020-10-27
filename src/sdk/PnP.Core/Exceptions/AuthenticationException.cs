using System;

namespace PnP.Core
{
    /// <summary>
    /// Authentication exception
    /// </summary>
    public class AuthenticationException : PnPException
    {
        #region Default exception constructors
        internal AuthenticationException()
        {
        }

        internal AuthenticationException(string message) : base(message)
        {
        }

        internal AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
        #endregion

        #region Custom constructors
        /// <summary>
        /// <see cref="AuthenticationException"/> constructor
        /// </summary>
        /// <param name="type">Type of error</param>
        /// <param name="message">Error message</param>
        public AuthenticationException(ErrorType type, string message) : base(message)
        {
            Error = new AuthenticationError(type, message);
        }

        /// <summary>
        /// <see cref="AuthenticationException"/> constructor
        /// </summary>
        /// <param name="type">Type of error</param>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception (if any)</param>
        public AuthenticationException(ErrorType type, string message, Exception innerException) : base(message, innerException)
        {
            Error = new AuthenticationError(type, message);
        }
        #endregion

        /// <summary>
        /// Outputs a <see cref="AuthenticationException"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Error?.ToString()}{Environment.NewLine}{base.ToString()}";
        }
    }
}
