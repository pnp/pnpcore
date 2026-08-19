using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    /// <summary>
    /// Custom authentication provider that uses the WebAssembly access token provider to obtain an access token
    /// </summary>
    public class MsalWrappedTokenProvider : IAuthenticationProvider
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="accessTokenProvider">WebAssembly access token provider instance</param>
        public MsalWrappedTokenProvider(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        private const string MicrosoftGraphScope = "Sites.FullControl.All";
        private const string SharePointOnlineScope = "AllSites.FullControl";

        private static string[] GetRelevantScopes(Uri resourceUri)
        {
            string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";

            if (resourceUri.DnsSafeHost.Equals("graph.microsoft.com", StringComparison.OrdinalIgnoreCase))
            {
                return [$"{resource}/{MicrosoftGraphScope}"];
            }

            return [$"{resource}/{SharePointOnlineScope}"];
        }

        /// <summary>
        /// Authenticate the web request
        /// </summary>
        /// <param name="resource">Resource to get an access token for</param>
        /// <param name="request">Request to add the access token on</param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            ArgumentNullException.ThrowIfNull(resource, nameof(resource));

            var accessToken = await GetAccessTokenAsync(resource, GetRelevantScopes(resource));
            request.Headers.Authorization = new ("bearer", accessToken);
        }

        /// <summary>
        /// Gets an access token for the requested resource and scopes
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <param name="scopes">Scopes to use when getting the access token</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            ArgumentNullException.ThrowIfNull(resource, nameof(resource));
            ArgumentNullException.ThrowIfNull(scopes, nameof(scopes));

            var tokenResult = await _accessTokenProvider.RequestAccessToken(new AccessTokenRequestOptions()
            {
                // The scopes must specify the needed permissions for the app to work
                Scopes = scopes,
            }).ConfigureAwait(false);

            if (!tokenResult.TryGetToken(out AccessToken? accessToken))
            {
                throw new InvalidOperationException(
                    $"Could not acquire an access token for {resource} using scopes '{string.Join(", ", scopes)}'. " +
                    $"Status: {tokenResult.Status}, interactive request url: {tokenResult.InteractiveRequestUrl ?? "none"}.");
            }

            return accessToken.Value;
        }

        /// <summary>
        /// Gets an access token for the requested resource
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource)
        {
            ArgumentNullException.ThrowIfNull(resource, nameof(resource));

            return await GetAccessTokenAsync(resource, GetRelevantScopes(resource));
        }
    }
}
