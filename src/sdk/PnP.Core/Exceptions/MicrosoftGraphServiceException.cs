using System;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Microsoft Graph Service exception
    /// </summary>
    public class MicrosoftGraphServiceException: ServiceException
    {
        #region Default exception constructors
        internal MicrosoftGraphServiceException(string message) : base(message)
        {
        }

        internal MicrosoftGraphServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal MicrosoftGraphServiceException()
        {
        }
        #endregion

        #region Custom constructors
        public MicrosoftGraphServiceException(ErrorType type, int httpResponseCode, string response): base("Microsoft Graph service exception")
        {            
            Error = new MicrosoftGraphError(type, httpResponseCode, response);            
        }

        public MicrosoftGraphServiceException(ErrorType type, int httpResponseCode, JsonElement error): base("Microsoft Graph service exception")
        {
            Error = new MicrosoftGraphError(type, httpResponseCode, error);
        }
        #endregion

        /// <summary>
        /// Outputs a <see cref="MicrosoftGraphServiceException"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Error?.ToString()}{Environment.NewLine}{base.ToString()}"; 
        }
    }
}
