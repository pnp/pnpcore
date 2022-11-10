using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using System.Net.Http;
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
                return new[] { $"{resourceUri}" };
            }
            else
            {
                string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";
                return new[] { $"{resource}" };
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
        /// Gets an access token for the requested resource and scopes
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <param name="scopes">Scopes to use when getting the access token</param>
        /// <returns>Obtained access token</returns>
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
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            return await GetAccessTokenAsync(resource, GetRelevantScopes(resource));
        }
    }
}