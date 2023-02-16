using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Model.Teams;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends the <see cref="IPnPContext"/> with additional functionality
    /// </summary>
    public class IPnPContextExtensionsImplementation : IPnPContextExtensions
    {
        #region Extend context with SharePoint Admin and Microsoft 365 admin capabilities
        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with SharePoint admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISharePointAdmin"/> instance enabling SharePoint admin operations</returns>
        public ISharePointAdmin GetSharePointAdmin(IPnPContext context)
        {
            return new SharePointAdmin(context as PnPContext);
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with site collection admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling site collection admin operations</returns>
        public ISiteCollectionManager GetSiteCollectionManager(IPnPContext context)
        {
            return new SiteCollectionManager(context as PnPContext);
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with Teams admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling site collection admin operations</returns>
        public ITeamManager GetTeamManager(IPnPContext context)
        {
            return new TeamManager(context as PnPContext);
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with tenant Application Lifecycle Management (ALM) functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ITenantAppManager"/> instance enabling tenant app catalog operations</returns>
        public ITenantAppManager GetTenantAppManager(IPnPContext context)
        {
            return new TenantAppManager(context as PnPContext);
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with site collection Application Lifecycle Management (ALM) functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionAppManager"/> instance enabling site collection app catalog operations</returns>
        public ISiteCollectionAppManager GetSiteCollectionAppManager(IPnPContext context)
        {
            return new SiteCollectionAppManager(context as PnPContext);
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with Microsoft 365 admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="IMicrosoft365Admin"/> instance enabling Microsoft 365 admin operations</returns>
        public IMicrosoft365Admin GetMicrosoft365Admin(IPnPContext context)
        {
            return new Microsoft365Admin(context as PnPContext);
        }

        #endregion

        #region Utilities
        internal static async Task WaitAsync(PnPContext context, TimeSpan timeToWait)
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
