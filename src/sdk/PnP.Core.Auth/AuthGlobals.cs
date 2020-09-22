using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Auth
{
    internal static class AuthGlobals
    {
        // Microsoft SharePoint Online Management Shell client id = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        // PnP Office 365 Management Shell = "31359c7f-bd7e-475c-86db-fdb8c937548e";
        /// <summary>
        /// Defines the default Client ID for apps that do not rely on their own Client ID
        /// </summary>
        internal const string DefaultClientId = "31359c7f-bd7e-475c-86db-fdb8c937548e";

        /// <summary>
        /// Defines the multi-tenant ID for multi-tenant apps
        /// </summary>
        internal const string OrganizationsTenantId = "organizations";

        /// <summary>
        /// The format string for the Authority of an OAuth request against AAD
        /// </summary>
        internal const string AuthorityFormat = "https://login.microsoftonline.com/{0}/";
    }
}
