using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Demo.PersistentTokenCache
{
    public class PersistentInteractiveAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// The Redirect URI for the authentication flow
        /// </summary>
        public Uri RedirectUri { get; set; }


        private IPublicClientApplication publicClientApplication;
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

        // Microsoft SharePoint Online Management Shell client id
        // private static readonly string aadAppId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        // PnP Office 365 Management Shell 

        /// <summary>
        /// Default Azure AD application id (PnP Management Shell)
        /// </summary>
        protected const string DefaultAADAppId = "31359c7f-bd7e-475c-86db-fdb8c937548e";

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        public PersistentInteractiveAuthenticationProvider(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            ClientId = options.ClientId;
            TenantId = options.TenantId;
            RedirectUri = options.Interactive?.RedirectUri;

            // Build the MSAL client
            publicClientApplication = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithPnPAdditionalAuthenticationSettings(
                    options.Interactive?.AuthorityUri,
                    RedirectUri,
                    TenantId)
                .Build();
        }

        /// <summary>
        /// Register a Msal persistent cache to handle tokens save and restore
        /// </summary>
        /// <param name="storageCreationProperties">Configured storage</param>
        /// <returns></returns>
        public async Task RegisterTokenStorageAsync(StorageCreationProperties storageCreationProperties)
        {
            var cacheHelper = await MsalCacheHelper.CreateAsync(storageCreationProperties).ConfigureAwait(false);
            cacheHelper.RegisterCache(publicClientApplication.UserTokenCache);
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
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
        public async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
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
                tokenResult = await publicClientApplication.AcquireTokenInteractive(scopes)
                    .ExecuteAsync().ConfigureAwait(false);
            }

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
        public async Task<string> GetAccessTokenAsync(Uri resource)
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
