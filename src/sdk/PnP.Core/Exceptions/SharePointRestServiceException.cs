using System;

namespace PnP.Core
{
    /// <summary>
    /// Microsoft Graph Service exception
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
        public SharePointRestServiceException(ErrorType type, int httpResponseCode, string response): base("SharePoint Rest service exception")
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
