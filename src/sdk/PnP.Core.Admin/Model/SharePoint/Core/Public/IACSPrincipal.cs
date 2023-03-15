using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Describes an Azure ACS principal
    /// </summary>
    public interface IACSPrincipal : ILegacyPrincipal
    {
        /// <summary>
        /// Id of the app in Azure AD
        /// </summary>
        Guid AppId { get; }

        /// <summary>
        /// Redirect URI used by the app
        /// </summary>
        string RedirectUri { get; }

        /// <summary>
        /// App domains used by the app
        /// </summary>
        string[] AppDomains { get; }

        /// <summary>
        /// Principal is valid until. This value is only populated when using the <see cref="ISiteCollectionManager.GetTenantAndSiteCollectionACSPrincipalsAsync(System.Collections.Generic.List{ILegacyServicePrincipal}, bool, VanityUrlOptions)"/> method
        /// </summary>
        DateTime ValidUntil { get; }
    }
}
