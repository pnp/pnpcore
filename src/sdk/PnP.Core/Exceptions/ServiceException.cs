using System;

namespace PnP.Core
{
    /// <summary>
    /// Abstract class representing service errors
    /// </summary>
    public class ServiceException: PnPException
    {
        /// <summary>
        /// Creates a service request exception
        /// </summary>
        /// <param name="message">Exception message</param>
        public ServiceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a service request exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception to be linked to this <see cref="ServiceException"/></param>
        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a service request exception
        /// </summary>
        public ServiceException()
        {
        }

        #region Custom constructors
        /// <summary>
        /// Creates a service request exception
        /// </summary>
        /// <param name="type">Error type</param>
        /// <param name="httpResponseCode">Http response code of the service request that got an error back</param>
        /// <param name="message">Exception message</param>
        public ServiceException(ErrorType type, int httpResponseCode, string message) : base(message)
        {
            Error = new ServiceError(type, httpResponseCode);
            (Error as ServiceError).Message = message;
        }
        #endregion
    }
}
