using System;

namespace PnP.Core
{
    /// <summary>
    /// Microsoft SharePoint REST Service exception
    /// </summary>
    public class SharePointRestServiceException : ServiceException
    {
        #region Default exception constructors
        internal SharePointRestServiceException(string message) : base(message)
        {
        }

        internal SharePointRestServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SharePointRestServiceException()
        {
        }
        #endregion

        #region Custom constructors
        /// <summary>
        /// Creates a <see cref="SharePointRestServiceException"/> using the provided error type, http response code and request response
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="httpResponseCode">Http response code of the service request</param>
        /// <param name="response">Response of the service request that errored out</param>
        public SharePointRestServiceException(ErrorType type, int httpResponseCode, string response) : base("SharePoint Rest service exception")
        {
            Error = new SharePointRestError(type, httpResponseCode, response);
        }
        #endregion

        /// <summary>
        /// Outputs a <see cref="SharePointRestServiceException"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Error?.ToString()}{Environment.NewLine}{base.ToString()}";
        }
    }
}
