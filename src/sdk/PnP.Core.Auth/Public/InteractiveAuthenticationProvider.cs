﻿using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Auth.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that uses an interactive flow prompting the user for credentials
    /// </summary>
    public class InteractiveAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The Redirect URI for the authentication flow
        /// </summary>
        public Uri RedirectUri { get; set; }

        // Instance private member, to keep the token cache at service instance level
        private IPublicClientApplication publicClientApplication;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="redirectUri">The Redirect URI for the authentication flow</param>
        public InteractiveAuthenticationProvider(string clientId, string tenantId, Uri redirectUri)
            : this(null)
        {
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                Interactive = new PnPCoreAuthenticationInteractiveOptions
                {
                    RedirectUri = redirectUri
                }
            });
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="options">Options for the authentication provider</param>
        public InteractiveAuthenticationProvider(string clientId, string tenantId, PnPCoreAuthenticationInteractiveOptions options)
            : this(null)
        {
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                Interactive = options
            });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interfafce
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public InteractiveAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            ClientId = !string.IsNullOrEmpty(options.ClientId) ? options.ClientId : AuthGlobals.DefaultClientId;
            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;
            RedirectUri = options.Interactive?.RedirectUri ?? AuthGlobals.DefaultRedirectUri;

            // Build the MSAL client
            publicClientApplication = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithPnPAdditionalAuthenticationSettings(options.Interactive, TenantId)
                .Build();

            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.InteractiveAuthenticationProvider_LogInit);
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
                tokenResult = await publicClientApplication.AcquireTokenInteractive(scopes)
                    .ExecuteAsync().ConfigureAwait(false);
            }

            // Log the access token retrieval action
            Log?.LogInformation(PnPCoreAuthResources.AuthenticationProvider_LogAccessTokenRetrieval,
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
