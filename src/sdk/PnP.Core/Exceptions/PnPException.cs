using System;

namespace PnP.Core
{
    /// <summary>
    /// Base class for the pnp core sdk exceptions
    /// </summary>
    public abstract class PnPException: Exception
    {
        #region Default constuctors
        public PnPException(string message) : base(message)
        {
        }

        public PnPException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public PnPException()
        {
        }
        #endregion

        /// <summary>
        /// Additional error information
        /// </summary>
        public BaseError Error { get; set; }
    }
}
