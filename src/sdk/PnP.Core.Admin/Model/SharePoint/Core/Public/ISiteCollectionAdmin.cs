using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines a site collection administrator
    /// </summary>
    public interface ISiteCollectionAdmin
    {
        /// <summary>
        /// Gets the id of the site collection administrator when the administrator is added due to being an Microsoft 365 Group owner
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the login name of the site collection administrator when the administrator is added as a SharePoint administrator
        /// </summary>
        public string LoginName { get; }

        /// <summary>
        /// Name of the admin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// User principle name (UPN) of the site collection administrator
        /// </summary>
        public string UserPrincipalName { get; }

        /// <summary>
        /// E-mail address of the site collection administrator
        /// SP REST property name: Email
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// Is this a secondary site collection administrator
        /// </summary>
        public bool IsSecondaryAdmin { get; }

        /// <summary>
        /// Site collection admin is also a Microsoft 365 group owner
        /// </summary>
        public bool IsMicrosoft365GroupOwner { get; }

        /// <summary>
        /// Set site collection admin as primary admin
        /// </summary>
        /// <returns></returns>
        Task SetAsPrimarySiteCollectionAdministratorAsync(Uri site);

        /// <summary>
        /// Set site collection admin as primary admin
        /// </summary>
        /// <returns></returns>
        void SetAsPrimarySiteCollectionAdministrator(Uri site);
    }
}
