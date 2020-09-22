using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider that uses the context of an ASP.NET Core web application
    /// </summary>
    public sealed class AspNetCoreAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger interfafce
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        public AspNetCoreAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        public override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the AspNetCore options
            if (options.AspNetCore == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.AspNetCoreAuthenticationProvider_InvalidConfiguration);
            }

            // TODO: Build the MSAL client
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public override Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public override Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public override Task<string> GetAccessTokenAsync(Uri resource)
        {
            throw new NotImplementedException();
        }
    }
}
