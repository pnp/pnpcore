using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Model.Teams;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends the <see cref="PnPContext"/> with additional functionality
    /// </summary>
    public interface IPnPContextExtensions
    {
        #region Extend context with SharePoint Admin and Microsoft 365 admin capabilities
        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with SharePoint admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISharePointAdmin"/> instance enabling SharePoint admin operations</returns>
        ISharePointAdmin GetSharePointAdmin(IPnPContext context);

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with site collection admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling site collection admin operations</returns>
        ISiteCollectionManager GetSiteCollectionManager(IPnPContext context);

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with Teams admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionManager"/> instance enabling site collection admin operations</returns>
        ITeamManager GetTeamManager(IPnPContext context);

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with tenant Application Lifecycle Management (ALM) functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ITenantAppManager"/> instance enabling tenant app catalog operations</returns>
        ITenantAppManager GetTenantAppManager(IPnPContext context);

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with site collection Application Lifecycle Management (ALM) functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="ISiteCollectionAppManager"/> instance enabling site collection app catalog operations</returns>
        ISiteCollectionAppManager GetSiteCollectionAppManager(IPnPContext context);

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with Microsoft 365 admin functionality
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="IMicrosoft365Admin"/> instance enabling Microsoft 365 admin operations</returns>
        IMicrosoft365Admin GetMicrosoft365Admin(IPnPContext context);

        #endregion
    }
}
