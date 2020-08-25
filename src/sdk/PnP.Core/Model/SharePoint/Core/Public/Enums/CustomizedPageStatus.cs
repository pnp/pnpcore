using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the cache and customization status for a page.
    /// </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1714:Flags enums should have plural names", Justification = "<Pending>")]
    public enum CustomizedPageStatus
    {
        /// <summary>
        /// Enumeration whose values specify that the page was never cached. The value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enumeration whose values specify that the page is cached and has not been customized. The value = 1.
        /// </summary>
        Uncustomized = 1,
        /// <summary>
        /// Enumeration whose values specify that the page was cached and has been customized. The value = 2.
        /// </summary>
        Customized = 2
    }
}
