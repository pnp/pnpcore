using System;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// CSOM Service exception
    /// </summary>
    public class CsomServiceException: ServiceException
    {
        #region Default exception constructors
        internal CsomServiceException(string message) : base(message)
        {
        }

        internal CsomServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal CsomServiceException()
        {
        }
        #endregion

        #region Custom constructors
        public CsomServiceException(ErrorType type, int httpResponseCode, JsonElement response) : base("CSOM service exception")
        {
            Error = new CsomError(type, httpResponseCode, response);            
        }
        #endregion

        /// <summary>
        /// Outputs a <see cref="CsomServiceException"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Error?.ToString()}{Environment.NewLine}{base.ToString()}";
        }
    }
}
