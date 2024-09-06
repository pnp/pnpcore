using PnP.Core.Services;
using System.Net.Http.Headers;
using Azure.Identity;

namespace Demo.AzFunction.ManagedIdentity
{
    public class ManagedIdentityTokenProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ManagedIdentityTokenProvider()
        {
        }

        private string[] GetRelevantScopes(Uri resourceUri)
        {
            if (resourceUri.ToString() == "https://graph.microsoft.com")
            {
                return [$"{resourceUri}"];
            }
            else
            {
                string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";
                return [$"{resource}"];
            }
        }

        /// <summary>
        /// Authenticate the web request
        /// </summary>
        /// <param name="resource">Resource to get an access token for</param>
        /// <param name="request">Request to add the access token on</param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentNullException.ThrowIfNull(resource);

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer",
                await GetAccessTokenAsync(resource).ConfigureAwait(false));
        }
        /// <summary>
        /// Gets an access token for the requested resource and scopes
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <param name="scopes">Scopes to use when getting the access token</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            ArgumentNullException.ThrowIfNull(resource);

            ArgumentNullException.ThrowIfNull(scopes);

            // var credential = new DefaultAzureCredential();
            var credential = new Azure.Identity.ChainedTokenCredential(
                 new ManagedIdentityCredential(),
                 new EnvironmentCredential()
                 );

            var accessToken = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes));

            return accessToken.Token;
        }
        /// <summary>
        /// Gets an access token for the requested resource
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource)
        {
            ArgumentNullException.ThrowIfNull(resource);

            return await GetAccessTokenAsync(resource, GetRelevantScopes(resource));
        }
    }
}