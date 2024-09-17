using System;

namespace PnP.Core.Auth
{
    internal static class AuthGlobals
    {
        /// <summary>
        /// Defines the multi-tenant ID for multi-tenant apps
        /// </summary>
        internal const string OrganizationsTenantId = "organizations";

        /// <summary>
        /// Defines the default Redirect URI for apps that do not rely on their own Redirect URI
        /// </summary>
        internal static readonly Uri DefaultRedirectUri = new Uri("http://localhost");

        /// <summary>
        /// The format string for the Authority of an OAuth request against AAD
        /// </summary>
        internal const string AuthorityFormat = "https://login.microsoftonline.com/{0}/";
    }
}
