using Microsoft.Identity.Client;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;

namespace PnP.Core.Auth.Utilities
{
    internal static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Helper method that configures an ApplicationBuilder object with the given options
        /// </summary>
        /// <typeparam name="T">The type built by the ApplicationBuilder</typeparam>
        /// <param name="builder">The extended ApplicationBuilder</param>
        /// <param name="authorityUri">The URI of the Azure Active Directory Authority</param>
        /// <param name="redirectUri">The Redirect URI for authentication</param>
        /// <param name="tenantId">The ID of the Azure Active Directory Tenant</param>
        /// <returns></returns>
        internal static T WithPnPAdditionalAuthenticationSettings<T>(this AbstractApplicationBuilder<T> builder,
            Uri authorityUri, Uri redirectUri, string tenantId) where T : AbstractApplicationBuilder<T>
        {
            if (tenantId == null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (tenantId.Equals(AuthGlobals.OrganizationsTenantId, StringComparison.InvariantCultureIgnoreCase))
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
