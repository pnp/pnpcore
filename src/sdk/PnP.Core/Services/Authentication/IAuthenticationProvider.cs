using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the public interface that any Authentication Provider must implement
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Get's the in use <see cref="IAuthenticationProviderConfiguration"/>
        /// </summary>
        IAuthenticationProviderConfiguration Configuration { get; }

        /// <summary>
        /// Configures the Authentication Provider
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        void Configure(IAuthenticationProviderConfiguration configuration);

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request);

        /// <summary>
        /// Get's an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        Task<string> GetAccessTokenAsync(Uri resource, String[] scopes);

        /// <summary>
        /// Get's an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        Task<string> GetAccessTokenAsync(Uri resource);
    }
}
