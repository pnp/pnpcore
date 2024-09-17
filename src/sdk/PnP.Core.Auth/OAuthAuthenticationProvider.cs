using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Base authentication provider class
    /// </summary>
    public abstract class OAuthAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// The Name of the configuration for the Authentication Provider
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Tenant ID for the application, default value is "organizations" for multi-tenant applications
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Local copy of the logger class
        /// </summary>
        internal readonly ILogger Log;

        /// <summary>
        /// Public constructor for any OAuth Authentication privider
        /// </summary>
        /// <param name="logger"></param>
        public OAuthAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
        {
            // logger can be null ... we don't necessarily need it
            Log = logger;
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        internal abstract void Init(PnPCoreAuthenticationCredentialConfigurationOptions options);

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public abstract Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request);

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public abstract Task<string> GetAccessTokenAsync(Uri resource, string[] scopes);

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public abstract Task<string> GetAccessTokenAsync(Uri resource);
    }
}
