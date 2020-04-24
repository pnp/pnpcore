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
        /// Configures the Authentication Provider
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        void Configure(IAuthenticationProviderConfiguration configuration);

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request);

        Task<string> GetAccessTokenAsync(Uri resource, String[] scopes);
    }
}
