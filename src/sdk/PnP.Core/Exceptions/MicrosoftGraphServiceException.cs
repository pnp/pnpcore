using System;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Microsoft Graph Service exception
    /// </summary>
    public class MicrosoftGraphServiceException : ServiceException
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
        /// <summary>
        /// Creates a <see cref="MicrosoftGraphServiceException"/> using the provided error type, http response code and request response
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="httpResponseCode">Http response code of the executed Graph request</param>
        /// <param name="response">Response of the executed Graph request</param>
        public MicrosoftGraphServiceException(ErrorType type, int httpResponseCode, string response) : base("Microsoft Graph service exception")
        {
            Error = new MicrosoftGraphError(type, httpResponseCode, response);
        }

        /// <summary>
        /// Creates a <see cref="MicrosoftGraphServiceException"/> using the provided error type, http response code and request response
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="httpResponseCode">Http response code of the executed Graph request</param>
        /// <param name="error">Json error coming from the executed Graph request</param>
        public MicrosoftGraphServiceException(ErrorType type, int httpResponseCode, JsonElement error) : base("Microsoft Graph service exception")
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
