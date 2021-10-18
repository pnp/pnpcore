using Microsoft.Identity.Client;
using System;

namespace Demo.PersistentTokenCache
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Defines the multi-tenant ID for multi-tenant apps
        /// </summary>
        internal const string OrganizationsTenantId = "organizations";
        public static T WithPnPAdditionalAuthenticationSettings<T>(this AbstractApplicationBuilder<T> builder,
            Uri authorityUri, Uri redirectUri, string tenantId) where T : AbstractApplicationBuilder<T>
        {
            if (tenantId == null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (tenantId.Equals(OrganizationsTenantId, StringComparison.InvariantCultureIgnoreCase))
            {
                builder = builder.WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs);
            }
            else
            {
                builder = builder.WithAuthority(authorityUri?.ToString() ?? "https://login.microsoftonline.com", tenantId, true);
            }

            if (redirectUri != null)
            {
                builder = builder.WithRedirectUri(redirectUri.ToString());
            }

            return (T)builder;
        }
    }
}
