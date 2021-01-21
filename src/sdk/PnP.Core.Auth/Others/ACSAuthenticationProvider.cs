using Microsoft.Extensions.Logging;
using PnP.Core.Auth.ACS;
using PnP.Core.Auth.ACS.OAuth;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that implements ACS token creation
    /// </summary>
    public sealed class ACSAuthenticationProvider : OAuthAuthenticationProvider
    {
        private OAuth2AccessTokenResponse lastToken;
        private TokenHelper tokenHelper;
        private Uri siteUrl;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="siteUrl">Url of the site collection to access</param>
        /// <param name="clientId">ClientId of the principal</param>
        /// <param name="options">Options to use for token creation</param>
        public ACSAuthenticationProvider(Uri siteUrl, string clientId, PnPCoreAuthenticationACSOptions options)
            : this((ILogger<OAuthAuthenticationProvider>)null)
        {
            this.siteUrl = siteUrl;
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions() { ClientId = clientId, ACS = options });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interface
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public ACSAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
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

            // realm is optional, determine it if not supplied
            if (string.IsNullOrEmpty(options.ACS.Realm))
            {
                options.ACS.Realm = TokenHelper.GetRealmFromTargetUrl(siteUrl).GetAwaiter().GetResult();
            }

            if (string.IsNullOrEmpty(options.ACS.Realm))
            {
                throw new Exception($"Could not determine realm for url {siteUrl}");
            }

            this.tokenHelper = new TokenHelper(options.ClientId, options.ACS);
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
            // token request without resource are unsupported
            if (resource == null)
            {
                return null;
            }

            // implement simple token caching
            if (lastToken == null || lastToken.ExpiresOn >= DateTime.Now.AddMinutes(-3))
            {
                lastToken = await tokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, resource.Authority).ConfigureAwait(false);
            }

            // Log the access token retrieval action
            Log?.LogInformation(PnPCoreAuthResources.ACSAuthenticationProvider_LogInit,
                GetType().Name, resource, scopes.Aggregate(string.Empty, (c, n) => c + ", " + n).TrimEnd(','));

            return lastToken.AccessToken;
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

            // We do net use scopes for ACS
            return await GetAccessTokenAsync(resource, null).ConfigureAwait(false);
        }
    }
}
