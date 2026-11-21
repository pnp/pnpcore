using System;

namespace PnP.Core.Modernization.Transform
{
#pragma warning disable CA1032
    /// <summary>
    /// Exception class thrown when a probed page does not exist in the web. Derives from
    /// <see cref="ArgumentException"/> so that existing handlers catching <see cref="ArgumentException"/>
    /// keep observing the previous behavior.
    /// </summary>
    public class PageDoesNotExistInWebException : ArgumentException
    {
        /// <summary>
        /// Throws a PageDoesNotExistInWebException message
        /// </summary>
        /// <param name="message">Error message</param>
        public PageDoesNotExistInWebException(string message) : base(message) { }

        /// <summary>
        /// Throws a PageDoesNotExistInWebException message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception object</param>
        public PageDoesNotExistInWebException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore CA1032
}
