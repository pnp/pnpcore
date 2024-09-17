using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
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
        private readonly UsernamePasswordAuthenticationProvider usernamePasswordProvider;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="options">Options for the authentication provider</param>
        public CredentialManagerAuthenticationProvider(string clientId, string tenantId,
            PnPCoreAuthenticationCredentialManagerOptions options)
            : this(null, null)
        {
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                CredentialManager = options
            });
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="credentialManagerName">The Name of the Credential Manager item for authentication</param>
        public CredentialManagerAuthenticationProvider(string clientId, string tenantId,
            string credentialManagerName)
            : this(clientId, tenantId, new PnPCoreAuthenticationCredentialManagerOptions
            {
                CredentialManagerName = credentialManagerName
            })
        {
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger and IMsalHttpClientFactory interfaces
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        /// <param name="msalHttpClientFactory">The instance of the Msal Http Client Factory service provided by DI</param>
        public CredentialManagerAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger, IMsalHttpClientFactory msalHttpClientFactory)
            : base(logger)
        {
            // Create an instance of the inner UsernamePasswordAuthenticationProvider
            usernamePasswordProvider = new UsernamePasswordAuthenticationProvider(logger, msalHttpClientFactory);
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

            if (credentials == null)
            {
                throw new ConfigurationErrorsException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                                    PnPCoreAuthResources.CredentialManagerAuthenticationProvider_CredentialManagerEntryDoesNotExist,
                                                                    CredentialManagerName));
            }

            if (!string.IsNullOrEmpty(options.ClientId))
            {
                ClientId = options.ClientId;
            }
            else
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.InvalidClientId);
            }

            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;

            var usernamePasswordOptions = new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = ClientId,
                TenantId = TenantId,
                Environment = options.Environment,
                AzureADLoginAuthority = options.AzureADLoginAuthority,
                UsernamePassword = new PnPCoreAuthenticationUsernamePasswordOptions
                {
                    Username = credentials.UserName,
                    Password = credentials.Password,
                }
            };

            // Configure the inner instance of UsernamePasswordAuthenticationProvider
            usernamePasswordProvider.Init(usernamePasswordOptions);

            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.CredentialManagerAuthenticationProvider_LogInit,
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
        /// Gets an access token for the requested resource and scope. Provide either scopes or resource parameter.
        /// </summary>
        /// <param name="resource">Resource to request an access token for, only used if scopes is null</param>
        /// <param name="scopes">Scopes to request, can be null</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

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
