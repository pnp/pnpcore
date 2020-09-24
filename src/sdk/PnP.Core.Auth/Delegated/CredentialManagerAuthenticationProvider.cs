using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that uses a set of credentials stored in the Credentials Manager of Windows
    /// </summary>
    public sealed class CredentialManagerAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The name of the Windows Credential Manager settings to use
        /// </summary>
        public string CredentialManagerName { get; set; }

        // Internally we use a Username and Password Authentication Provider
        private UsernamePasswordAuthenticationProvider usernamePasswordProvider;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenand ID for the Authentication Provider</param>
        /// <param name="credentialManagerName">The Name of the Credential Manager item for authentication</param>
        public CredentialManagerAuthenticationProvider(string clientId, string tenantId,
            string credentialManagerName)
            : this(null)
        {
            this.Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                CredentialManager = new PnPCoreAuthenticationCredentialManagerOptions
                {
                    CredentialManagerName = credentialManagerName
                }
            });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interfafce
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public CredentialManagerAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
            // Create an instance of the inner UsernamePasswordAuthenticationProvider
            usernamePasswordProvider = new UsernamePasswordAuthenticationProvider(logger);
        }


        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the CredentialManager options
            if (options.CredentialManager == null)
            {
                throw new ArgumentException(
                    PnPCoreAuthResources.CredentialManagerAuthenticationProvider_InvalidConfiguration,
                    nameof(options));
            }

            if (string.IsNullOrEmpty(options.CredentialManager.CredentialManagerName))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.CredentialManagerAuthenticationProvider_InvalidCredentialManagerName);
            }

            // Retrieve credentials from the Credential Manager
            CredentialManagerName = options.CredentialManager.CredentialManagerName;
            var credentials = CredentialManager.GetCredential(CredentialManagerName);

            ClientId = !string.IsNullOrEmpty(options.ClientId) ? options.ClientId : AuthGlobals.DefaultClientId;
            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;

            var usernamePasswordOptions = new PnPCoreAuthenticationCredentialConfigurationOptions {
                ClientId = ClientId,
                TenantId = TenantId,
                UsernamePassword = new PnPCoreAuthenticationUsernamePasswordOptions
                {
                    Username = credentials.UserName,
                    Password = credentials.Password,
                }
            };

            // Configure the inner instance of UsernamePasswordAuthenticationProvider
            usernamePasswordProvider.Init(usernamePasswordOptions);

            // Log the initialization information
            this.Log?.LogInformation(PnPCoreAuthResources.CredentialManagerAuthenticationProvider_LogInit,
                options.CredentialManager.CredentialManagerName);
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public override async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            await usernamePasswordProvider.AuthenticateRequestAsync(resource, request).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (scopes == null)
            {
                // If scopes are missing, let's rely on the internal 
                // UsernamePasswordAuthenticationProvider just providing the resource
                return await usernamePasswordProvider.GetAccessTokenAsync(resource).ConfigureAwait(false);
            }
            else
            {
                // Otherwise use the internal UsernamePasswordAuthenticationProvider 
                // method the whole set of argument
                return await usernamePasswordProvider.GetAccessTokenAsync(resource, scopes).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource)
        {
            return await usernamePasswordProvider.GetAccessTokenAsync(resource).ConfigureAwait(false);
        }
    }
}
