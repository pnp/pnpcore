using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Model.SharePoint;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends the <see cref="PnPContext"/> with additional functionality
    /// </summary>
    public static class PnPContextExtensions
    {
        #region Extend context with SharePoint Admin and Microsoft 365 admin capabilities
        /// <summary>
        /// Extends a <see cref="PnPContext"/> with SharePoint admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="ISharePointAdmin"/> instance enabling SharePoint admin operations</returns>
        public static ISharePointAdmin GetSharePointAdmin(this PnPContext context)
        {
            return new SharePointAdmin(context);
        }

        /// <summary>
        /// Extends a <see cref="PnPContext"/> with Microsoft 365 admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="IMicrosoft365Admin"/> instance enabling Microsoft 365 admin operations</returns>
        public static IMicrosoft365Admin GetMicrosoft365Admin(this PnPContext context)
        {
            return new Microsoft365Admin(context);
        }

        #endregion

    }
}
