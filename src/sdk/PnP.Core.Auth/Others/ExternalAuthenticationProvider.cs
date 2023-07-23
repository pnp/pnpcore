using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that relies on an external token provider 
    /// </summary>
    public sealed class ExternalAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// A function providing the access token to use
        /// </summary>
        public Func<Uri, string[], string> AccessTokenProvider { get; set; }

        /// <summary>
        /// A function providing the access token to use
        /// </summary>
        public Func<Uri, string[], Task<string>> AccessTokenTaskProvider { get; set; }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="accessTokenProvider">A function providing the access token to use</param>
        public ExternalAuthenticationProvider(Func<Uri, string[], string> accessTokenProvider)
            : this((ILogger<OAuthAuthenticationProvider>)null)
        {
            AccessTokenProvider = accessTokenProvider;
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="accessTokenProvider">A function providing the access token to use</param>
        public ExternalAuthenticationProvider(Func<Uri, string[], Task<string>> accessTokenProvider)
            : this((ILogger<OAuthAuthenticationProvider>)null)
        {
            AccessTokenTaskProvider = accessTokenProvider;
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interfafce
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public ExternalAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.ExternalAuthenticationProvider_LogInit);
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
        /// <param name="resource">Resource to request an access token for</param>
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

            if (AccessTokenProvider == null && AccessTokenTaskProvider == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.ExternalAuthenticationProvider_MissingAccessTokenProvider);
            }

            var accessToken = AccessTokenProvider?.Invoke(resource, scopes) ?? (await AccessTokenTaskProvider.Invoke(resource, scopes).ConfigureAwait(false));

            // Log the access token retrieval action
            Log?.LogInformation(PnPCoreAuthResources.AuthenticationProvider_LogAccessTokenRetrieval,
                GetType().Name, resource, scopes.Aggregate(string.Empty, (c, n) => c + ", " + n).TrimEnd(','));

            // Return the Access Token, if we've got it
            return accessToken;
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
