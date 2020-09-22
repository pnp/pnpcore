using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider based on the OnBehalfOf flow
    /// </summary>
    public sealed class OnBehalfOfAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The ClientSecret to authenticate the app with ClientId
        /// </summary>
        public SecureString ClientSecret { get; set; }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenand ID for the Authentication Provider</param>
        /// <param name="clientSecret">The Client Secret of the app</param>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public OnBehalfOfAuthenticationProvider(string clientId, string tenantId,
            SecureString clientSecret,
            ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
            this.Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                OnBehalfOf = new PnPCoreAuthenticationOnBehalfOfOptions
                {
                    ClientSecret = clientSecret.ToInsecureString()
                }
            }); ;
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interfafce
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public OnBehalfOfAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the OnBehalfOf options
            if (options.OnBehalfOf == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_InvalidConfiguration);
            }

            // We need the certificate thumbprint
            if (string.IsNullOrEmpty(options.OnBehalfOf.ClientSecret))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_InvalidClientSecret);
            }

            ClientId = !string.IsNullOrEmpty(options.ClientId) ? options.ClientId : AuthGlobals.DefaultClientId;
            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;
            ClientSecret = options.OnBehalfOf.ClientSecret.ToSecureString();

            // TODO: Build the MSAL client

            // Log the initialization information
            this.Log?.LogInformation(PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_LogInit);
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public override Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public override Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            throw new NotImplementedException();

            //// Log the access token retrieval action
            //this.Log?.LogInformation(PnPCoreAuthResources.AuthenticationProvider_LogAccessTokenRetrieval,
            //    this.GetType().Name, resource, scopes.Aggregate(string.Empty, (c, n) => c + ", " + n).TrimEnd(','));
        }

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public override Task<string> GetAccessTokenAsync(Uri resource)
        {
            throw new NotImplementedException();
        }
    }
}
