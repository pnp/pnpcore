using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Model.SharePoint;
using System;
using System.Threading.Tasks;

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
        /// Extends a <see cref="PnPContext"/> with site collection admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling site collection admin operations</returns>
        public static ISiteCollectionManager GetSiteCollectionManager(this PnPContext context)
        {
            return new SiteCollectionManager(context);
        }

        /// <summary>
        /// Extends a <see cref="PnPContext"/> with app admin functionality
        /// </summary>
        /// <param name="context"><see cref="PnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling app admin operations</returns>
        public static IAppManager GetAppManager(this PnPContext context)
        {
            return new AppManager(context);
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

        #region Utilities
        internal static async Task WaitAsync(this PnPContext context, TimeSpan timeToWait)
        {
#if DEBUG
            if (context.Mode == TestMode.Mock)
            {
                // No point in waiting when we're running the test as offline test
            }
            else
            {
                await Task.Delay(timeToWait).ConfigureAwait(false);
            }
#else
            await Task.Delay(timeToWait).ConfigureAwait(false);
#endif
        }
        #endregion
    }
}
