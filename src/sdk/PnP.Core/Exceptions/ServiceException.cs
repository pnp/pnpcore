using System;

namespace PnP.Core
{
    /// <summary>
    /// Abstract class representing service errors
    /// </summary>
    public class ServiceException: PnPException
    {
        public ServiceException(string message) : base(message)
        {
        }

        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServiceException()
        {
        }

        #region Custom constructors
        public ServiceException(ErrorType type, int httpResponseCode, string message) : base(message)
        {
            Error = new ServiceError(type, httpResponseCode);
            (Error as ServiceError).Message = message;
        }
        #endregion
    }
}
