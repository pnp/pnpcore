using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Auth.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that uses a Resource Owner Password Credentials (ROCP) credential flow
    /// </summary>
    /// <remarks>
    /// You can find further details about ROPC here: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth-ropc
    /// </remarks>
    public sealed class UsernamePasswordAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The username for authenticating
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password for authenticating
        /// </summary>
        public SecureString Password { get; set; }

        // Instance private member, to keep the token cache at service instance level
        private IPublicClientApplication publicClientApplication;

        // Instance private member, to keep the Msal Http Client Factory at service instance level
        private readonly IMsalHttpClientFactory msalHttpClientFactory;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="username">The Username for authentication</param>
        /// <param name="password">The Password for authentication</param>
        public UsernamePasswordAuthenticationProvider(string clientId, string tenantId,
            string username, SecureString password)
            : this(clientId, tenantId, new PnPCoreAuthenticationUsernamePasswordOptions
            {
                Username = username,
                Password = password?.ToInsecureString()
            })
        {
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="options">Options for the authentication provider</param>
        public UsernamePasswordAuthenticationProvider(string clientId, string tenantId,
            PnPCoreAuthenticationUsernamePasswordOptions options)
            : this(null, null)
        {
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                UsernamePassword = options
            });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger and IMsalHttpClientFactory interfaces
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        /// <param name="msalHttpClientFactory">The instance of the Msal Http Client Factory service provided by DI</param>
        public UsernamePasswordAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger, IMsalHttpClientFactory msalHttpClientFactory)
            : base(logger)
        {
            this.msalHttpClientFactory = msalHttpClientFactory;
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the UsernamePassword options
            if (options.UsernamePassword == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.UsernamePasswordAuthenticationProvider_InvalidConfiguration);
            }

            // We need the Username
            if (string.IsNullOrEmpty(options.UsernamePassword.Username))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.UsernamePasswordAuthenticationProvider_InvalidUsername);
            }

            // We need the Password
            if (string.IsNullOrEmpty(options.UsernamePassword.Password))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.UsernamePasswordAuthenticationProvider_InvalidPassword);
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
            Username = options.UsernamePassword.Username;
            Password = options.UsernamePassword.Password.ToSecureString();

            // Build the MSAL client
            publicClientApplication = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithPnPAdditionalAuthenticationSettings(
                    options.UsernamePassword.AuthorityUri,
                    options.UsernamePassword.RedirectUri,
                    TenantId,
                    options.Environment,
                    options.AzureADLoginAuthority)
                .WithHttpClientFactory(msalHttpClientFactory)
                .Build();

            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.UsernamePasswordAuthenticationProvider_LogInit,
                options.UsernamePassword.Username);
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public override async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer",
                await GetAccessTokenAsync(resource).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for (unused)</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            AuthenticationResult tokenResult = null;

            var account = await publicClientApplication.GetAccountsAsync().ConfigureAwait(false);
            try
            {
                // Try to get the token from the tokens cache
                tokenResult = await publicClientApplication.AcquireTokenSilent(scopes, account.FirstOrDefault())
                    .ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                // Try to get the token directly through AAD if it is not available in the tokens cache
#pragma warning disable CS0618 // Type or member is obsolete
                tokenResult = await publicClientApplication.AcquireTokenByUsernamePassword(scopes, Username, Password)
                    .ExecuteAsync().ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // Log the access token retrieval action
            Log?.LogDebug(PnPCoreAuthResources.AuthenticationProvider_LogAccessTokenRetrieval,
                GetType().Name, resource, scopes.Aggregate(string.Empty, (c, n) => c + ", " + n).TrimEnd(','));

            // Return the Access Token, if we've got it
            // In case of any exception while retrieving the access token, 
            // MSAL will throw an exception that we simply bubble up
            return tokenResult.AccessToken;
        }

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            // Use the .default scope if the scopes are not provided
            return await GetAccessTokenAsync(resource,
                new string[] { $"{resource.Scheme}://{resource.Authority}/.default" }).ConfigureAwait(false);
        }
    }
}
