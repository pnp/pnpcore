using System;

namespace PnP.Core
{
    /// <summary>
    /// Base class for the pnp core sdk exceptions
    /// </summary>
    public abstract class PnPException : Exception
    {
        #region Default constuctors

        /// <summary>
        /// Creates a PnP Exception
        /// </summary>
        /// <param name="message">Exception message</param>
        public PnPException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a PnP Exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception to be linked to this <see cref="PnPException"/></param>
        public PnPException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a PnP Exception
        /// </summary>
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
