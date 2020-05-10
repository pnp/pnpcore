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
        public ClientException(ErrorType type, string message) : base(message)
        {
            Error = new ClientError(type, message);
        }

        public ClientException(ErrorType type, string message, Exception innerException) : base(message, innerException)
        {
            Error = new ClientError(type, message);
        }
        #endregion
    }
}
