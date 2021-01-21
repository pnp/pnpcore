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
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <param name="tenantid"></param>
        /// <returns></returns>
        public static T WithPnPAdditionalAuthenticationSettings<T>(this AbstractApplicationBuilder<T> builder,
            PnPCoreAuthenticationBaseOptions options, string tenantid) where T : AbstractApplicationBuilder<T>
        {
            if (tenantid.Equals(AuthGlobals.OrganizationsTenantId, StringComparison.InvariantCultureIgnoreCase))
            {
                builder = builder.WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs);
            }
            else
            {
                builder = builder.WithAuthority(options.AuthorityUri.ToString() ?? "https://login.microsoftonline.com", tenantid, true);
            }

            if (options.RedirectUri != null)
            {
                builder = builder.WithRedirectUri(options.RedirectUri.ToString());
            }

            return (T)builder;
        }
    }
}
