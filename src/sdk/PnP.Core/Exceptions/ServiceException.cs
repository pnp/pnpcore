using System;

namespace PnP.Core
{
    /// <summary>
    /// Abstract class representing service errors
    /// </summary>
    public abstract class ServiceException: PnPException
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
    }
}
